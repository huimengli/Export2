using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export.Attribute;
using Export.Enums;
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
        [ReadOnly]
        public string uuid;

        /// <summary>
        /// 控制器类型
        /// </summary>
        [ReadOnly]
        public ControlEnum control = ControlEnum.Keyboard;

        /// <summary>
        /// 静步移动速度
        /// </summary>
        [Range(0.1f,5.0f)]
        public float quiteWalkSpeed = 2.0f; // 静步移动速度

        /// <summary>
        /// 玩家移动速度
        /// </summary>
        [Range(2f,10f)]
        public float moveSpeed = 5.0f; // 玩家移动速度

        /// <summary>
        /// 冲刺速度
        /// </summary>
        [Range(5f,20f)]
        public float runSpeed = 10.0f; // 冲刺速度

        /// <summary>
        /// 当前移动速度
        /// </summary>
        [ReadOnly]
        public float currentMoveSpeed = 0;

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
        [ReadOnly]
        public List<KeyCode> moveKeys = new List<KeyCode>();

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

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

        public float CurrentMoveSpeed
        {
            get { return currentMoveSpeed; }
            set { currentMoveSpeed = value; }
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
            // 设置移动速度
            currentMoveSpeed = MoveSpeed;
        }

        private void Update()
        {
            // 这里控制玩家的移动速度逻辑
            if (Input.GetKeyDown(sprintKey))
            {
                currentMoveSpeed = RunSpeed; // 按下冲刺键时设置为冲刺速度
            }
            else if (Input.GetKeyDown(quietWalkKey))
            {
                currentMoveSpeed = quiteWalkSpeed; // 按下静步按键时设置为静步速度
            }
            // 这里可以添加玩家移动的逻辑
            // 例如，获取输入并更新玩家位置
            var input = false;
            foreach (var key in moveKeys)
            {
                if (Input.GetKey(key))
                {
                    input = true;
                    break;
                }
            }
            if (input)
            {
                Move();
            }
        }

        public Vector3 Move()
        {
            // 获取输入
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            // 计算移动方向
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            // 应用速度
            movement *= currentMoveSpeed * Time.deltaTime;
            // 更新玩家位置
            transform.Translate(movement);
            return movement;
        }
    }
}
