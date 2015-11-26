using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Stormancer;
using Stormancer.Core;
using System;

namespace Stormancer.Chat
{
    public class ChatUserInfo
    {
        public string User;
    }

    public struct ChatMessageDTO
    {
        public ChatUserInfo UserInfo;
        public string Message;
    }

    public class SimpleChatBehaviour : StormancerIRemoteLogic
    {
        public Text ChatTxt;
        public InputField PlayerTxt;
        public Button SendBtn;
        public Slider chatSlider;
        public int messageToShowNbr = 17;

        public string UserInfo;
        private ChatUserInfo Infos;

        private bool messageReceived = false;
        private ConcurrentQueue<string> ReceiverPump = new ConcurrentQueue<string>();
        private List<string> MessagePump = new List<string>();

        public override void Init(Scene s)
        {
            s.AddRoute("chat", OnChat);
            SendBtn.onClick.AddListener(OnSendMessage);
            chatSlider.onValueChanged.AddListener(ShowMessages);
            Infos = new ChatUserInfo();
            Infos.User = UserInfo;
        }

        public override void OnConnected()
        {
            RemoteScene.Scene.Send<ChatUserInfo>("UpdateInfo", Infos);
        }

        public void OnChat(Packet<IScenePeer> packet)
        {
            ChatMessageDTO dto;

            dto = packet.ReadObject<ChatMessageDTO>();
            if (dto.UserInfo == null)
            {
                dto.UserInfo = new ChatUserInfo();
                dto.UserInfo.User = "John Doe";
            }
            string message = dto.UserInfo.User + ": " + dto.Message + "\b";
            ReceiverPump.Enqueue(message);

        }

        public void OnSendMessage()
        {
            var message = PlayerTxt.text;
            RemoteScene.Scene.Send<string>("chat", message);
            PlayerTxt.text = "";
        }

        public void ShowMessages(float flt)
        {
            int i;
            int j;

            j = 0;
            i = MessagePump.Count - (int)chatSlider.value;
            if (i < 0)
            {
                i = 0;
            }

            ChatTxt.text = "";

            while (i + j < MessagePump.Count - 1 && j < messageToShowNbr)
            {
                ChatTxt.text += MessagePump[i + j] + "\b";
            }
        }

        void Update()
        {
            if (messageReceived == true)
            {
                string temp;
                while (ReceiverPump.Count > 0)
                {
                    ReceiverPump.TryDequeue(out temp);
                    MessagePump.Add(temp);
                    if (MessagePump.Count >= 100)
                    {
                        MessagePump.RemoveAt(0);
                    }
                }
                ShowMessages(0);
            }
        }
    }
}
