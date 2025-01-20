using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;
    //����һ��˽�в����ֶΣ����ڱ�ǳ�ʼ���Ƿ����
    [Header("Card")]
    public Card ����Ƭ;
    //����һ�������ֶΣ����ڴ洢����Ƭ�����ã�[Header("Card")]������Unity�༭����Ϊ�ֶη���
    private Transform ��Ƭ�任;
    private Vector3 ��ת����;
    private int ��������;
    Vector3 �ƶ�����;
    private Canvas canvas;
    //��Щ��˽���ֶΣ����ڴ洢��Ƭ�ı任����ת������������������ƶ�������Canvas���
    [Header("References")]
    public Transform �Ӿ���Ӱ;//����һ�������ֶΣ����ڴ洢�Ӿ���Ӱ�ı任
    private float ��Ӱƫ�� = 20;
    private Vector2 ��Ӱ����;
    private Canvas ��ӰCanvas;
    [SerializeField] private Transform ҡ��������;
    [SerializeField] private Transform ��б������;
    [SerializeField] private Image ��Ƭͼ��;
    //��Щ��˽���ֶΣ����ڴ洢��Ӱƫ�ơ���Ӱ���롢��ӰCanvas��ҡ����������б������Ϳ�Ƭͼ������á�
    [Header("Follow Parameters")]
    [SerializeField] private float �����ٶ� = 30;
    //����һ�����л���˽���ֶΣ��������ø����ٶ�
    [Header("Rotation Parameters")]
    [SerializeField] private float ��ת�� = 20;
    [SerializeField] private float ��ת�ٶ� = 20;
    //��Щ�����л���˽���ֶΣ�����������ת�������ٶ�
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;
    
    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    //��Щ�����л���˽���ֶΣ������������Ŷ����Ĳ���
    [Header("Select Parameters")]
    [SerializeField] private float ѡ��ʱ�ĳ���� = 20;
    //����һ�����л���˽���ֶΣ���������ѡ��ʱ�ĳ����
    [Header("Hober Parameters")]
    [SerializeField] private float ��ͣʱ�ĳ���Ƕ� = 5;
    [SerializeField] private float ��ͣʱ����ʱ�� = .15f;
    //��Щ�����л���˽���ֶΣ�����������ͣʱ�ĳ���ǶȺ͹���ʱ��
    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;
    //��Щ�����л���˽���ֶΣ��������ý��������Ĳ���
    [Header("Curve")]
    [SerializeField] private CurveParameters curve;
    //����һ�����л���˽���ֶΣ����ڴ洢���߲���
    private float curveYOffset;
    private float curveRotationOffset;
    private Coroutine pressCoroutine;
    //��Щ��˽���ֶΣ����ڴ洢���ߵ�Yƫ�ƺ���תƫ�ƣ��Լ�һ��Э������
    private void Start()
    {//Start�����ڽű�ʵ����ʱ���ã����ڳ�ʼ����Ӱ����
        ��Ӱ���� = �Ӿ���Ӱ.localPosition;
    }

    public void Initialize(Card target, int index = 0)
    {//����һ���������������ڳ�ʼ��CardVisual��������ø���Ƭ���¼���������
        //Declarations
        ����Ƭ = target;
        ��Ƭ�任 = target.transform;
        canvas = GetComponent<Canvas>();
        ��ӰCanvas = �Ӿ���Ӱ.GetComponent<Canvas>();

        //Event Listening
        ����Ƭ.PointerEnterEvent.AddListener(PointerEnter);
        ����Ƭ.PointerExitEvent.AddListener(PointerExit);
        ����Ƭ.BeginDragEvent.AddListener(BeginDrag);
        ����Ƭ.EndDragEvent.AddListener(EndDrag);
        ����Ƭ.PointerDownEvent.AddListener(PointerDown);
        ����Ƭ.PointerUpEvent.AddListener(PointerUp);
        ����Ƭ.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;
    }

    public void UpdateIndex(int length)
    {//����һ���������������ڸ��¿�Ƭ���Ӿ�����
        transform.SetSiblingIndex(����Ƭ.transform.parent.GetSiblingIndex());
    }

    void Update()
    {//ÿ֡���ã�����ִ���ֵ�λ�á�ƽ�����桢������ת�Ϳ�Ƭ��б���߼�
        if (!initalize || ����Ƭ == null) return;

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();

    }

    private void HandPositioning()
    {//����������ڴ����ֵ�λ���߼�
        curveYOffset = (curve.positioning.Evaluate(����Ƭ.NormalizedPosition()) * curve.positioningInfluence) * ����Ƭ.SiblingAmount();
        curveYOffset = ����Ƭ.SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(����Ƭ.NormalizedPosition());
    }

    private void SmoothFollow()
    {//����������ڴ���ƽ�������߼�
        Vector3 verticalOffset = (Vector3.up * (����Ƭ.�Ƿ������϶� ? 0 : curveYOffset));
        transform.position = Vector3.Lerp(transform.position, ��Ƭ�任.position + verticalOffset, �����ٶ� * Time.deltaTime);
    }

    private void FollowRotation()
    {//����������ڴ��������ת�߼�
        Vector3 movement = (transform.position - ��Ƭ�任.position);
        �ƶ����� = Vector3.Lerp(�ƶ�����, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (����Ƭ.�Ƿ������϶� ? �ƶ����� : movement) * ��ת��;
        ��ת���� = Vector3.Lerp(��ת����, movementRotation, ��ת�ٶ� * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(��ת����.x, -60, 60));
    }

    private void CardTilt()
    {//����������ڴ���Ƭ��б�߼�
        �������� = ����Ƭ.�Ƿ������϶� ? �������� : ����Ƭ.ParentIndex();
        float sine = Mathf.Sin(Time.time + ��������) * (����Ƭ.�Ƿ���ͣ ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + ��������) * (����Ƭ.�Ƿ���ͣ ? .2f : 1);

        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tiltX = ����Ƭ.�Ƿ���ͣ ? ((offset.y * -1) * manualTiltAmount) : 0;
        float tiltY = ����Ƭ.�Ƿ���ͣ ? ((offset.x) * manualTiltAmount) : 0;
        float tiltZ = ����Ƭ.�Ƿ������϶� ? ��б������.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * ����Ƭ.SiblingAmount()));

        float lerpX = Mathf.LerpAngle(��б������.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(��б������.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(��б������.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        ��б������.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    private void Select(Card card, bool state)
    {//����������ڴ���ѡ���߼�
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        ҡ��������.DOPunchPosition(ҡ��������.up * ѡ��ʱ�ĳ���� * dir, scaleTransition, 10, 1);
        ҡ��������.DOPunchRotation(Vector3.forward * (��ͣʱ�ĳ���Ƕ� / 2), ��ͣʱ����ʱ��, 20, 1).SetId(2);

        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

    }

    public void Swap(float dir = 1)
    {//����һ���������������ڴ������߼�
        if (!swapAnimations)
            return;

        DOTween.Kill(2, true);
        ҡ��������.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
    }

    private void BeginDrag(Card card)
    {//����������ڴ���ʼ�϶��߼�
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    private void EndDrag(Card card)
    {//����������ڴ�������϶��߼�
        canvas.overrideSorting = false;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(Card card)
    {//����������ڴ���ָ������߼�
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        ҡ��������.DOPunchRotation(Vector3.forward * ��ͣʱ�ĳ���Ƕ�, ��ͣʱ����ʱ��, 20, 1).SetId(2);
    }

    private void PointerExit(Card card)
    {//����������ڴ���ָ���뿪�߼�
        if (!����Ƭ.�Ƿ��϶���)
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(Card card, bool longPress)
    {//����������ڴ���ָ���ͷ��߼�
        if (scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
        canvas.overrideSorting = false;

        �Ӿ���Ӱ.localPosition = ��Ӱ����;
        ��ӰCanvas.overrideSorting = true;
    }

    private void PointerDown(Card card)
    {//����������ڴ���ָ�밴���߼�
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        �Ӿ���Ӱ.localPosition += (-Vector3.up * ��Ӱƫ��);
        ��ӰCanvas.overrideSorting = false;
    }

}

