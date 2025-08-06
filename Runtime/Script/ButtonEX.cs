using Export.Attribute;
using Export.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

using Object = UnityEngine.Object;

namespace Export.Script
{
    /// <summary>
    /// 增强版按钮，支持长按、双击、触摸开始、触摸结束、触摸移动等事件
    /// </summary>
    [AddComponentMenu("UI/ButtonEX")]
    public class ButtonEX : Button
    {
        // 事件定义
        [SerializeField] private UnityEvent onLongPress;
        [SerializeField] private UnityEvent onDoubleClick;
        [SerializeField] private UnityEvent onTouchStart;
        [SerializeField] private UnityEvent onTouchEnd;
        [SerializeField] private UnityEvent onTouchMove;

        // 配置参数
        [SerializeField] private float longPressDuration = 0.5f;
        [SerializeField] private float doubleClickInterval = 0.3f;

        // 内部状态
        private bool isPointerDown;
        private float pointerDownTime;
        private int clickCount;
        private Coroutine longPressCoroutine;
        private Coroutine doubleClickCoroutine;

        // 长按事件
        public UnityEvent OnLongPress => onLongPress;

        // 双击事件
        public UnityEvent OnDoubleClick => onDoubleClick;

        // 触摸开始事件
        public UnityEvent OnTouchStart => onTouchStart;

        // 触摸结束事件
        public UnityEvent OnTouchEnd => onTouchEnd;

        // 触摸移动事件
        public UnityEvent OnTouchMove => onTouchMove;

        // 长按时间阈值
        public float LongPressDuration
        {
            get => longPressDuration;
            set => longPressDuration = Mathf.Max(0.1f, value);
        }

        // 双击间隔时间
        public float DoubleClickInterval
        {
            get => doubleClickInterval;
            set => doubleClickInterval = Mathf.Max(0.1f, value);
        }

        protected override void Awake()
        {
            base.Awake();
            clickCount = 0;
        }

        #region 指针事件处理
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            isPointerDown = true;
            pointerDownTime = Time.unscaledTime;

            // 启动长按检测
            longPressCoroutine = StartCoroutine(LongPressDetection());

            // 触发触摸开始事件
            onTouchStart?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            isPointerDown = false;

            // 停止长按检测
            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                longPressCoroutine = null;
            }

            // 触发触摸结束事件
            onTouchEnd?.Invoke();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // 长按后不触发点击
            if (!isPointerDown) return;

            clickCount++;

            // 第一次点击
            if (clickCount == 1)
            {
                doubleClickCoroutine = StartCoroutine(DoubleClickDetection());
            }
            // 第二次点击（双击）
            else if (clickCount == 2)
            {
                ResetDoubleClick();
                onDoubleClick?.Invoke();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            // 离开时重置状态
            if (isPointerDown)
            {
                isPointerDown = false;
                if (longPressCoroutine != null)
                {
                    StopCoroutine(longPressCoroutine);
                    longPressCoroutine = null;
                }
                onTouchEnd?.Invoke();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            onTouchMove?.Invoke();
        }
        #endregion

        #region 协程处理
        private IEnumerator LongPressDetection()
        {
            yield return new WaitForSecondsRealtime(longPressDuration);

            // 如果仍然按住
            if (isPointerDown)
            {
                onLongPress?.Invoke();
                isPointerDown = false; // 防止触发点击事件
            }
        }

        private IEnumerator DoubleClickDetection()
        {
            yield return new WaitForSecondsRealtime(doubleClickInterval);

            // 超时未双击，触发普通点击
            if (clickCount == 1)
            {
                base.OnPointerClick(new PointerEventData(EventSystem.current));
                ResetDoubleClick();
            }
        }
        #endregion

        #region 辅助方法
        private void ResetDoubleClick()
        {
            clickCount = 0;
            if (doubleClickCoroutine != null)
            {
                StopCoroutine(doubleClickCoroutine);
                doubleClickCoroutine = null;
            }
        }
        #endregion
    }
}
