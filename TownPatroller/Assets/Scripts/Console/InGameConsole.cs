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

        private uint CreatePos;
        private const uint NextAddPos = 55;

        public InGameConsole(GameObject consoleContent, Text textPrefab)
        {
            ConsoleContent = consoleContent;
            TextPrefab = textPrefab;

            TextOBJs = new LinkedList<Text>();
            CreatePos = 0;
        }

        private void CreateTextPrefabs(int createcount)
        {

        }

        public void println(string msg)
        {
            TextOBJs.AddLast(Instantiate(TextPrefab, ConsoleContent.transform));
            TextOBJs.Last.Value.rectTransform.text = msg;
            //TextOBJs.Last.Value.rectTransform.rect = new Rect(new Vector2(TextOBJs.Last.Value.rectTransform.rect.x, CreatePos), TextOBJs.Last.Value.rectTransform.rect.size);
            //CreatePos += NextAddPos;
            //for (int i = 0; i < msg.Length % 31; i++)
            //{
            //    CreatePos += NextAddPos;
            //}
        }
        public void print()
        {

        }
    }
}
