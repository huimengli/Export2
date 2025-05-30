using Export.Attribute;
using Export.BehaviourEX;
using UnityEngine;

namespace Export
{
    /// <summary>
    /// 消息框
    /// 这里使用Dispatcher来替换MonoBehaviour
    /// </summary>
    public class Message : MonoBehaviour
    {
        /// <summary>
        /// 是否使用Form来获取输入
        /// </summary>
        public bool UseForm;

        /// <summary>
        /// 这里不写内容
        /// 是丢给编译器给别人看消息用的
        /// </summary>
        [SerializeField]
        [ReadOnlyTextArea]
        public string value;

        /// <summary>
        /// 修改文字内容
        /// </summary>
        /// <param name="text"></param>
        public void ChangeValue(string text)
        {
            this.value = text;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
