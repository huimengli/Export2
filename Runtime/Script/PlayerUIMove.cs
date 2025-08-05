using Export.Attribute;
using Export.Interface;
using Export.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Export.Script
{
    /// <summary>
    /// 玩家触控移动类
    /// </summary>
    internal class PlayerUIMove : MonoBehaviour, IPlayerMove
    {
        /// <summary>
        /// 部件唯一标识符
        /// </summary>
        [ReadOnly]
        public string uuid;

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
        /// 摇杆图片
        /// </summary>
        public RawImage rawImage; // 移动摇杆图片

        /// <summary>
        /// 摇杆按钮图片
        /// </summary>
        public RawImage buttonImage; // 摇杆按钮图片

        /// <summary>
        /// 摇杆可移动半径
        /// </summary>
        [Range(10f,200f)]
        public float buttonImageRadius = 150f; // 摇杆可移动半径

        /// <summary>
        /// 摇杆按钮位置
        /// </summary>
        public (float,float) buttonImagePosition = (0f, 0f); // 摇杆按钮位置

        /// <summary>
        /// 当前移动速度
        /// </summary>
        public float currentMoveSpeed = 0f; // 当前移动速度

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

        public string UUID
        {
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
            uuid = Item.NewUUID(); // 生成唯一标识符
            buttonImagePosition = (0f, 0f); // 初始化摇杆按钮位置
        }

        private void Start()
        {
            // 设置移动速度
            currentMoveSpeed = moveSpeed;

            // 设置按钮方法
            var rawButton = rawImage.rectTransform.GetComponent<Button>();
            if (rawButton == null)
            {
                rawImage.gameObject.AddComponent<Button>();
                rawButton = rawImage.rectTransform.GetComponent<Button>();
            }

            rawButton.onClick.AddListener(() => 
            {
                // 摇杆按钮点击事件
                // 这里可以添加摇杆按钮点击后的逻辑
                Debug.Log("摇杆按钮被点击");
            });
        }

        private void Update()
        {
            // 这里可以添加触控移动的逻辑
            // 例如，检测触摸输入并更新摇杆按钮位置
            // 目前暂时不实现具体的触控逻辑
        }

        public Vector3 Move()
        {
            return Vector3.zero; // 触控移动方法，暂时返回零向量
        }
    }
}
