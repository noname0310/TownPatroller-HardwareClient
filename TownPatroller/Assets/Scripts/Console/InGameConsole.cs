using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TownPatroller.Console
{
    class InGameConsole : MonoBehaviour
    {
        private GameObject ConsoleContent;
        private Text TextPrefab;
        private LinkedList<Text> TextOBJs;
        private LinkedList<Text> ActiveTextOBJs;

        private ScrollRect scrollRect;

        private const int CreateTickCount = 10;
        private const int MaxCount = 100;

        private int CreatePos;

        public void _new(GameObject consoleContent, Text textPrefab, ScrollRect scrollrect)
        {
            ConsoleContent = consoleContent;
            TextPrefab = textPrefab;
            scrollRect = scrollrect;

            TextOBJs = new LinkedList<Text>();
            ActiveTextOBJs = new LinkedList<Text>();
            CreatePos = 0;
        }

        private void CreateTextPrefabs(int createcount)
        {
            for (int i = 0; i < createcount; i++)
            {
                TextOBJs.AddFirst(Instantiate(TextPrefab, ConsoleContent.transform));
                TextOBJs.First.Value.gameObject.SetActive(false);
            }
        }

        private void GetOlderText()
        {
            CreatePos -= (int)ActiveTextOBJs.Last.Value.rectTransform.sizeDelta.y;

            for (int i = 0; i < ConsoleContent.transform.childCount; i++)
            {
                ConsoleContent.transform.GetChild(i).GetComponent<RectTransform>().transform.position = new Vector3(
                    ConsoleContent.transform.GetChild(i).GetComponent<RectTransform>().transform.position.x,
                    ConsoleContent.transform.GetChild(i).GetComponent<RectTransform>().position.y + 56/*(int)ActiveTextOBJs.Last.Value.rectTransform.sizeDelta.y*/,
                    ConsoleContent.transform.GetChild(i).GetComponent<RectTransform>().transform.position.z);
            }

            ActiveTextOBJs.Last.Value.rectTransform.transform.position = new Vector3(
                ActiveTextOBJs.Last.Value.rectTransform.transform.position.x,
                TextPrefab.rectTransform.transform.position.y,
                ActiveTextOBJs.Last.Value.rectTransform.transform.position.z);

            ConsoleContent.GetComponent<RectTransform>().sizeDelta = new Vector2(ConsoleContent.GetComponent<RectTransform>().sizeDelta.x, CreatePos);
            scrollRect.verticalNormalizedPosition = 0;

            Text text = ActiveTextOBJs.Last.Value;
            ActiveTextOBJs.RemoveLast();

            ActiveTextOBJs.AddFirst(text);
        }

        public void println(string msg)
        {
            if(ActiveTextOBJs.Count >= MaxCount)
            {
                GetOlderText();
            }
            else
            {
                if (TextOBJs.Count == 0)
                    CreateTextPrefabs(CreateTickCount);

                Text text = TextOBJs.Last.Value;
                TextOBJs.RemoveLast();

                ActiveTextOBJs.AddFirst(text);
            }

            ActiveTextOBJs.First.Value.gameObject.SetActive(true);
            ActiveTextOBJs.First.Value.text = msg;
            ActiveTextOBJs.First.Value.rectTransform.transform.position = new Vector3(
                ActiveTextOBJs.First.Value.rectTransform.transform.position.x, 
                ActiveTextOBJs.First.Value.rectTransform.transform.position.y - CreatePos, 
                ActiveTextOBJs.First.Value.rectTransform.transform.position.z);

            CreatePos += 56;//(int)ActiveTextOBJs.First.Value.rectTransform.sizeDelta.y;
            ConsoleContent.GetComponent<RectTransform>().sizeDelta = new Vector2(ConsoleContent.GetComponent<RectTransform>().sizeDelta.x, CreatePos);

            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
