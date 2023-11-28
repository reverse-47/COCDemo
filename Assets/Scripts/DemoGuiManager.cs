using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DemoGuiManager : MonoBehaviour
{
    public Button send_whisper;
    public Image wait_whisper;
    public TMP_InputField whisper_content;
    public TMP_Text sysInfoText;
    public TMP_Text skill_name;
    public TMP_Text skill_num;
    public TMP_Text skill_num_for_compare;
    public TMP_Text skill_num_random;
    public TMP_Text step_content;
    public TMP_Text completed;
    public Button dialog_but;
    public int maxTextWidth = 500;
    public int maxTextHeight = 400;
    
    public void DisplaySysInfo(string message)
    {
        sysInfoText.text = message;
    }

    public void DisplaySkillInfo(string name, int num, int compare_num)
    {
        skill_name.text = name+" - ";
        if(num > compare_num){
            skill_num_random.text = " > "+compare_num.ToString();
            SetTextColor(skill_num,"#7DC662");
            SetTextColor(skill_num_for_compare,"#7DC662");
        }
        else{
            skill_num_random.text = " < "+compare_num.ToString();
            SetTextColor(skill_num,"#EC4D47");
            SetTextColor(skill_num_for_compare,"#EC4D47");
        }
        skill_num.text = num.ToString();
        skill_num_for_compare.text = num.ToString();
    }

    public IEnumerator DisplayDialog(string rolename, string dialog)
    {
        GameObject role = GameObject.Find(rolename);
        TMP_Text dialog_content = dialog_but.GetComponentInChildren<TMP_Text>();
        RectTransform dialogRectTransform = dialog_but.GetComponent<RectTransform>();
        dialogRectTransform.sizeDelta = new Vector2(maxTextWidth, maxTextHeight);
        dialog_content.text = dialog;
        dialog_content.ForceMeshUpdate();
        float adjustedWidth = Mathf.Clamp(dialog.Length*40f, 100f, 500f);
        float adjustedHeight = Mathf.Clamp(dialog.Length/16*40f, 80f, 400f);

        // 更新文本框的大小
        dialogRectTransform.sizeDelta = new Vector2(adjustedWidth, adjustedHeight);

        // 设置文本框的位置
        Vector3 rolePosition = role.transform.position;
        Vector3 dialogPosition = rolePosition + new Vector3(1f, 1.5f, 0f); // 假设UI显示在角色正上方1.5个单位的高度
        Vector3 dialogScreenPosition = Camera.main.WorldToScreenPoint(dialogPosition);
        Canvas canvas = dialog_but.GetComponentInParent<Canvas>();
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), dialogScreenPosition, canvas.worldCamera, out canvasPosition);
        dialogRectTransform.localPosition = canvasPosition;
        
        // 设置文本框显示时长
        dialog_but.gameObject.SetActive(true);
        float minDelay = 2f;
        float maxDelay = 6f;
        float delayPerCharacter = 0.1f;
        float preferredDelay = dialog.Length * delayPerCharacter;
        float adjustedDelay = Mathf.Clamp(preferredDelay, minDelay, maxDelay);

        yield return new WaitForSeconds(adjustedDelay);
        dialog_but.gameObject.SetActive(false);
    }

    public IEnumerator DisplayDialogCoroutine(string rolename, string dialog, System.Action callback)
    {
        yield return StartCoroutine(DisplayDialog(rolename, dialog));
        callback.Invoke();
    }

    public void SetTextColor(TMP_Text text, string hexColor)
    {
        // 将字符串颜色值转换为 Color 对象
        Color color;
        if (ColorUtility.TryParseHtmlString(hexColor, out color))
        {
            // 设置文本的颜色
            text.color = color;
        }
        else
        {
            Debug.LogError("无效的颜色值: " + hexColor);
        }
    }

    // public string GetEmojiFromUnicode(string unicode)
    // {
    //     // 将 Unicode 表情代码转换为字符串
    //     string emojiString = char.ConvertFromUtf32(int.Parse(unicode, System.Globalization.NumberStyles.HexNumber));
    //     return emojiString;
    // }
}