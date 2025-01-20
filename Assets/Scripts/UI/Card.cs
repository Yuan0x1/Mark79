
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool �Ƿ�ʵ�����Ӿ������� = true;
    private VisualCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float ��Ƭ�ƶ����ٶ����� = 50;

    [Header("Selection")]
    public bool ��ѡ��;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject ��Ƭ�Ӿ�Ԥ�Ƽ�;
    [HideInInspector] public CardVisual ��Ƭ�Ӿ�������;
    //��Щ�ֶ����ڿ�Ƭ���Ӿ����֣�����Ԥ�Ƽ����Ӿ�������������
    [Header("States")]
    public bool �Ƿ���ͣ;
    public bool �Ƿ������϶�;
    [HideInInspector] public bool �Ƿ��϶���;
    //��Щ�ֶ����ڸ��ٿ�Ƭ��״̬�����Ƿ���ͣ���Ƿ������϶��Լ��Ƿ��϶���
    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;
    //��Щ��UnityEvent�ֶΣ�������Unity�༭���з���ط����¼�������
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!�Ƿ�ʵ�����Ӿ�������)
            return;

        visualHandler = FindObjectOfType<VisualCardsHandler>();
        ��Ƭ�Ӿ������� = Instantiate(��Ƭ�Ӿ�Ԥ�Ƽ�, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        ��Ƭ�Ӿ�������.Initialize(this);
    }
    //Start�����ڽű�ʵ����ʱ���ã����ڳ�ʼ��Canvas��Image��������ã��Լ�ʵ�����Ӿ�������
    //Ҳ����˵�������ʼ����֮������һ�ѵ�Canvas��Image�����ͬʱÿ��card��λ��װ��һ���Ӿ����Ŀ���
    void Update()
    {
        ClampPosition();

        if (�Ƿ������϶�)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(��Ƭ�ƶ����ٶ�����, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }
    //���ƿ�Ƭ��λ�ò������϶��߼���ÿ֡��
    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }
    //// ���ƿ�Ƭλ������Ļ��Χ��
    public void OnBeginDrag(PointerEventData eventData)
    {// ����ʼ�϶��¼�
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        �Ƿ������϶� = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        �Ƿ��϶��� = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {// ��������϶��¼�
        EndDragEvent.Invoke(this);
        �Ƿ������϶� = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            �Ƿ��϶��� = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {// ����ָ������¼�
        PointerEnterEvent.Invoke(this);
        �Ƿ���ͣ = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {// ����ָ���뿪�¼�
        PointerExitEvent.Invoke(this);
        �Ƿ���ͣ = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {// ����ָ�밴���¼�
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {// ����ָ���ͷ��¼�
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (�Ƿ��϶���)
            return;

        ��ѡ�� = !��ѡ��;
        SelectEvent.Invoke(this, ��ѡ��);

        if (��ѡ��)
            transform.localPosition += (��Ƭ�Ӿ�������.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }
    //��Щ����ʵ���˽ӿ��ж�����¼������߼������ڴ����û����϶���ָ�뽻��
    public void Deselect()
    {// ȡ��ѡ��Ƭ
        if (��ѡ��)
        {
            ��ѡ�� = false;
            if (��ѡ��)
                transform.localPosition += (��Ƭ�Ӿ�������.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {// ��ȡͬ��Ԫ������
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {// ��ȡ��Ԫ�ص�����
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {// ��ȡ��׼��λ��
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {// �ڶ�������ʱ���ã���������
        if (��Ƭ�Ӿ������� != null)
            Destroy(��Ƭ�Ӿ�������.gameObject);
    }
}

