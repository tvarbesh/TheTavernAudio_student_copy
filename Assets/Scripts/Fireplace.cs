using UnityEngine;
using FMODUnity;

public class FireplaceController : MonoBehaviour
{
    [Header("Настройки FMOD")]
    public EventReference fireplaceEvent;

    [Header("Ссылки")]
    [Tooltip("Перетащи сюда объект Player из иерархии")]
    public Transform player;

    [Header("Настройки этажей")]
    [Tooltip("Высота, с которой начинается второй этаж")]
    public float floorThresholdY = -5.36f; // Твое значение

    [Tooltip("Скорость затухания звука")]
    public float fadeSpeed = 2.0f;

    // Внутренние переменные
    private FMOD.Studio.EventInstance fireplaceInstance;
    private float currentVolume = 1f;

    void Start()
    {
        // Создаем инстанс камина при старте игры
        if (!fireplaceEvent.IsNull)
        {
            fireplaceInstance = RuntimeManager.CreateInstance(fireplaceEvent);

            // Привязываем звук к 3D-координатам объекта камина, чтобы он звучал из правильного места
            RuntimeManager.AttachInstanceToGameObject(fireplaceInstance, transform, GetComponent<Rigidbody>());

            // Запускаем звук (он будет играть бесконечно, если в FMOD настроен Loop)
            fireplaceInstance.start();
        }
    }

    void Update()
    {
        // Защита от ошибок, если не назначен игрок
        if (player == null || !fireplaceInstance.isValid()) return;

        // Определяем, какую громкость мы хотим сейчас (1 - максимум, 0 - тишина)
        // Если игрок выше или на уровне -5.36, целевая громкость 0. Иначе 1.
        float targetVolume = (player.position.y >= floorThresholdY) ? 0f : 1f;

        // Плавно меняем текущую громкость в сторону целевой
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, fadeSpeed * Time.deltaTime);

        // Применяем громкость к инстансу FMOD
        fireplaceInstance.setVolume(currentVolume);
    }

    // Очищаем память при выходе из игры или удалении объекта
    void OnDestroy()
    {
        if (fireplaceInstance.isValid())
        {
            fireplaceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            fireplaceInstance.release();
        }
    }
}