using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export.Interface;
using Export.Tools;
using UnityEngine;

namespace Export.Script
{
    /// <summary>
    /// 玩家移动类
    /// </summary>
    public class PlayerKeybordMove : MonoBehaviour, IPlayerMove
    {
        /// <summary>
        /// 部件唯一标识符
        /// </summary>
        private string uuid;

        /// <summary>
        /// 静步移动速度
        /// </summary>
        public float quiteWalkSpeed = 2.0f; // 静步移动速度

        /// <summary>
        /// 玩家移动速度
        /// </summary>
        public float moveSpeed = 5.0f; // 玩家移动速度

        /// <summary>
        /// 冲刺速度
        /// </summary>
        public float runSpeed = 10.0f; // 冲刺速度

        public KeyCode moveForwardKey = KeyCode.W; // 前进键
        public KeyCode moveBackwardKey = KeyCode.S; // 后退键
        public KeyCode moveLeftKey = KeyCode.A; // 左移键
        public KeyCode moveRightKey = KeyCode.D; // 右移键

        public KeyCode jumpKey = KeyCode.Space; // 跳跃键
        public KeyCode sprintKey = KeyCode.LeftControl; // 冲刺键
        public KeyCode stabilityKey = KeyCode.Space; // 稳定键
        public KeyCode quietWalkKey = KeyCode.LeftAlt; // 静音行走键

        /// <summary>
        /// 移动时的控制键列表
        /// </summary>
        public List<KeyCode> moveKeys = new List<KeyCode>();

        public string UUID {
            get { return uuid; }
        }

        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }

        public float QuiteWalkSpeed
        {
            get { return quiteWalkSpeed; }
            set { quiteWalkSpeed = value; }
        }

        public float RunSpeed
        {
            get { return runSpeed; }
            set { runSpeed = value; }
        }

        private void Awake()
        {
            uuid = Item.NewUUID();

            // 初始化移动键列表
            moveKeys.Add(moveForwardKey);
            moveKeys.Add(moveBackwardKey);
            moveKeys.Add(moveLeftKey);
            moveKeys.Add(moveRightKey);
        }

        private void Start()
        {

        }

        private void Update()
        {
            // 这里可以添加玩家移动的逻辑
            // 例如，获取输入并更新玩家位置

        }

        public Vector3 Move()
        {
            // 获取输入
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            // 计算移动方向
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            // 应用速度
            movement *= MoveSpeed * Time.deltaTime;
            // 更新玩家位置
            transform.Translate(movement);
            return movement;
        }
    }
}
