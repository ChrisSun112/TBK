using System.Xml;

namespace Tbk.Entity
{
    public class ExmlMsg
    {
        /// <summary>
        /// 本公众账号
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// 用户账号
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// 发送时间戳
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 发送的文本内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息的类型
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }

        public string EventKey { get; set; }

        public static ExmlMsg GetExmlMsg(XmlElement root)
        {
            ExmlMsg xmlMsg = new ExmlMsg()
            {
                FromUserName = root.SelectSingleNode("FromUserName").InnerText,
                ToUserName = root.SelectSingleNode("ToUserName").InnerText,
                CreateTime = root.SelectSingleNode("CreateTime").InnerText,
                MsgType = root.SelectSingleNode("MsgType").InnerText,
            };
            if (xmlMsg.MsgType.Trim().ToLower() == "text")
            {
                xmlMsg.Content = root.SelectSingleNode("Content").InnerText;
            }
            else if (xmlMsg.MsgType.Trim().ToLower() == "event")
            {
                xmlMsg.EventName = root.SelectSingleNode("Event").InnerText;

                if (xmlMsg.EventName!=null&&xmlMsg.EventName.Trim().ToLower() == "click")
                {
                    xmlMsg.EventKey = root.SelectSingleNode("EventKey").InnerText;
                }
            }
           
            return xmlMsg;
        }
    }
}