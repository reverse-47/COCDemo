using System;
using GrpcCOCDemo;
using Grpc.Core;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public DemoClient Client;
    public DemoGuiManager GuiManager;
    public GameObject player;
    public GameObject barrier;
    public bool is_barrier_open;
    public bool is_game_over;
    public RepeatedField<GrpcCOCDemo.Skill> skill_list;
    public RepeatedField<GrpcCOCDemo.Object> env_info_list;
    public RepeatedField<GrpcCOCDemo.Object> last_memory;
    int story_id = 1001;
    public FollowTarget navigation;
    public bool isNavigating;

    void Start()
    {
        StartCoroutine(init_object_list());
        barrier.tag = "close";     
        is_barrier_open = false;    
        is_game_over = false;                                                                                                                                                           
    }

    public void Update()
    {
        if (isNavigating && !navigation.nav.pathPending && navigation.nav.remainingDistance <= navigation.nav.stoppingDistance)
        {
            isNavigating = false;
        }
        if (barrier.tag == "guard")
        {
            isNavigating = false;
            barrier.tag = "close";
        }
        if(barrier.tag == "open" && !is_barrier_open)
        {
            is_barrier_open = true;
            GuiManager.DisplaySysInfo("You can enter the bar counter now.");
            for(int i =0; i<env_info_list.Count; i++)
            {
                if(env_info_list[i].ObjectInfo.Id == 1001207)
                {
                    env_info_list[i].LastStatus = "open";
                }
            }
            for(int i =0; i<last_memory.Count; i++)
            {
                if(last_memory[i].ObjectInfo.Id == 1001207)
                {
                    last_memory[i].LastStatus = "open";
                }
            }
        }
        var distance = Vector3.Distance(player.transform.position, barrier.transform.position);
        // print(distance);
        if (distance <0.5 && !is_game_over){
            is_game_over = true;
            GuiManager.DisplaySysInfo("You find bloody graffiti on the wall that says 'Aren't you glad you didn't turn on the light'");
            GuiManager.completed.gameObject.SetActive(true);
        }
    }
    private IEnumerator WaitForNavigationComplete(System.Action onComplete)
    {
        // 等待直到寻路完成
        while (isNavigating)
        {
            yield return null;
        }

        Debug.Log("Navigation completed.");
        
        onComplete?.Invoke();
    }
    
    
    public IEnumerator init_object_list()
    {
        GetObjectListRequest getObjectListRequest = new GetObjectListRequest { StoryId = story_id };
        var getObjectListTask = Client.client.GetObjectListAsync(getObjectListRequest);
        var awaiter = getObjectListTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var response = awaiter.GetResult();
            print(response);
            last_memory = new RepeatedField<GrpcCOCDemo.Object>();
            env_info_list= response.ObjectList;
            
            //add nana to list
            GrpcCOCDemo.Object Nana = new GrpcCOCDemo.Object {ObjectInfo = new DataPair {Id = -1, Name= "Nana"}, ObjectType = 1, IsKeyObject = false, LastStatus = "Enter the bar", ResourseName = "Nana"};
            env_info_list.Add(Nana);
            StartCoroutine(init_skill_list());
        });
        yield return null;
    }

    public IEnumerator init_skill_list()
    {
        GetSkillListRequest getSkillListRequest = new GetSkillListRequest { UserId = 1 };
        var getSkillListTask = Client.client.GetSkillListAsync(getSkillListRequest);
        var awaiter = getSkillListTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var response = awaiter.GetResult();
            print(response);
            skill_list= response.SkillList;
            GuiManager.send_whisper.onClick.AddListener(() => Onsent());
        });
        yield return null;
    }

    public void Onsent()
    {
        StartCoroutine(CheckFeasible());
        GuiManager.send_whisper.gameObject.SetActive(false);
        GuiManager.wait_whisper.gameObject.SetActive(true);
    }


    public IEnumerator CheckFeasible()
    {
        CheckFeasibleRequest checkFeasibleRequest = new CheckFeasibleRequest { StoryId = story_id, WhisperContent = GuiManager.whisper_content.text };
        var checkFeasibleTask = Client.client.CheckFeasibleAsync(checkFeasibleRequest);
        var awaiter = checkFeasibleTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var response = awaiter.GetResult();
            print(response);
            if(response.IsFeasible)
            {
                StartCoroutine(GetSkillUsed());
            }
            else{
                GuiManager.DisplaySysInfo("Nana seems to have a mind of her own...");
                GuiManager.send_whisper.gameObject.SetActive(true);
                GuiManager.wait_whisper.gameObject.SetActive(false);
            }
        });
        yield return null;
    }

    public IEnumerator GetSkillUsed()
    {
        GetSkillUsedRequest getSkillUsedRequest = new GetSkillUsedRequest { WhisperContent = GuiManager.whisper_content.text };
        var getSkillUsedTask = Client.client.GetSkillUsedAsync(getSkillUsedRequest);
        var awaiter = getSkillUsedTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var response = awaiter.GetResult();
            print(response);
            // show skill information
            bool skill_state = true;
            if(response.Skill.Id == -1)
            {
                GuiManager.DisplaySysInfo("You don't need to use skill.");
            }
            else{
                System.Random random = new System.Random();
                int randomNumber = random.Next(0, 101);
                int skill_num = 0;
                for(int i =0; i<skill_list.Count; i++){
                    if(skill_list[i].SkillInfo.Id == response.Skill.Id){
                        skill_num = skill_list[i].Num;
                        break;
                    }
                }
                var skill_info = "You are going to use skill: "+response.Skill.Name;
                GuiManager.DisplaySysInfo(skill_info);
                GuiManager.DisplaySkillInfo(response.Skill.Name, skill_num, randomNumber);
                skill_state = CompareSkill(skill_num, randomNumber);
            }
            StartCoroutine(GetObjectReply(skill_state));
        });
        yield return null;
    }

    public IEnumerator GetObjectReply(bool skill_state)
    {
        // select part of envinfolist
        AppendKeyItem();
        GetObjectReplyRequest getObjectReplyRequest = new GetObjectReplyRequest { StoryId = story_id, WhisperContent = GuiManager.whisper_content.text, EnvInfoList = last_memory, SkillState = skill_state };
        var getObjectReplyTask = Client.client.GetObjectReplyAsync(getObjectReplyRequest);
        var awaiter = getObjectReplyTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var response = awaiter.GetResult();
            print(response);
            last_memory = response.ReplyList;
            StartCoroutine(GetObjectsAction(0, response.ReplyList));
        });
        yield return null;
    }

    public IEnumerator GetObjectsAction(int point, RepeatedField<GrpcCOCDemo.Object> gameobjects)
    {
        string rolename = " ";
        if(point < gameobjects.Count)
        {
            for( int i = 0; i<env_info_list.Count; i++)
            {
                if(env_info_list[i].ObjectInfo.Id == gameobjects[point].ObjectInfo.Id)
                {
                    env_info_list[i].LastStatus = gameobjects[point].LastStatus;
                    rolename = env_info_list[i].ResourseName;
                }
            }
            if(gameobjects[point].ObjectType == 1)
            {
                GetObjectActionRequest getObjectActionRequest = new GetObjectActionRequest { Object = gameobjects[point] };
                var getObjectActionTask = Client.client.GetObjectActionAsync(getObjectActionRequest);
                var awaiter = getObjectActionTask.GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    var response = awaiter.GetResult();
                    print(response);
                    string temp_content = "😀"; //

                    if (response.Action.ActionObject != null)
                    {
                        for( int i = 0; i<env_info_list.Count; i++)
                        {
                            if(env_info_list[i].ObjectInfo.Id == response.Action.ActionObject.Id)
                            {
                                //nav
                                print(rolename+" should find "+env_info_list[i].ResourseName);
                                navigation.MoveToObject(rolename, env_info_list[i].ResourseName);
                                isNavigating = true;
                                StartCoroutine(WaitForNavigationComplete(() =>
                                {
                                    navigation.nav.isStopped = true;
                                    // print("stop nav");
                                    navigation.nav.ResetPath();
                                    if(response.Action.ActionType == 101){
                                        temp_content = response.Action.TalkContent;
                                    }
                                    else{
                                        temp_content = response.Action.EmojiList[0].Name;
                                    }
                                    StartCoroutine(GuiManager.DisplayDialogCoroutine(rolename, temp_content, () =>
                                    {
                                        point++;
                                        StartCoroutine(GetObjectsAction(point, gameobjects));
                                    }));
                                }));
                            }
                        }
                    }
                    else
                    {
                        temp_content = response.Action.EmojiList[0].Name;
                        StartCoroutine(GuiManager.DisplayDialogCoroutine(rolename, temp_content, () =>
                        {
                            point++;
                            StartCoroutine(GetObjectsAction(point, gameobjects));
                        }));
                    }
                });
            }
            else if (gameobjects[point].ObjectType == 2)
            {
                // if key object match object state
                point++;
                StartCoroutine(GetObjectsAction(point, gameobjects));
            }
        }
        else{
            GuiManager.send_whisper.gameObject.SetActive(true);
            GuiManager.wait_whisper.gameObject.SetActive(false);
        }
        yield return null;
    }

    public void AppendKeyItem()
    {
        bool is_key_item_exist = false;
        for(int i =0; i<last_memory.Count; i++){
            if (last_memory[i].ObjectInfo.Id == 1001207)
            {
                is_key_item_exist = true;
            }
        }
        if(!is_key_item_exist){
            for(int i =0; i<env_info_list.Count; i++)
            {
                if(env_info_list[i].ObjectInfo.Id == 1001207)
                {
                    var temp_info = env_info_list[i];
                    last_memory.Add(temp_info);
                }
            }
        }
    }

    public void MatchObjectState(){
        // algorithm  api
        // to be done
    }

    public bool CompareSkill(int skill_num, int random_num)
    {
        if(skill_num > random_num)
            return true;
        else
            return false;
    }
}
