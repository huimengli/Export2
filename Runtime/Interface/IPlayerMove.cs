using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Export.Interface
{
    /// <summary>
    /// 玩家移动接口
    /// </summary>
    public interface IPlayerMove
    {
        /// <summary>
        /// 部件唯一标识符
        /// </summary>
        string UUID { get; }

        /// <summary>
        /// 静步移动速度
        /// </summary>
        float QuiteWalkSpeed { get; set; }

        /// <summary>
        /// 玩家移动速度
        /// </summary>
        float MoveSpeed { get; set; }

        /// <summary>
        /// 冲刺速度
        /// </summary>
        float RunSpeed { get; set; } 

        /// <summary>
        /// 当前移动速度
        /// </summary>
        float CurrentMoveSpeed { get; set; }

        /// <summary>
        /// 玩家位置
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// 玩家旋转
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// 玩家移动方法
        /// </summary>
        /// <returns></returns>
        Vector3 Move();
    }
}
