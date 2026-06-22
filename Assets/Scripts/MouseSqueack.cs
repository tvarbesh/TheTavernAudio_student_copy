using UnityEngine;
using FMODUnity;

public class MouseSqueak : MonoBehaviour
{
    [Header("Настройки звука FMOD")]
    public EventReference squeakEvent;

    [Tooltip("Снапшот, который включается, когда игрок в комнате")]
    public EventReference mouseRoomSnapshot; // Поле для снапшота

    [Header("Ссылки")]
    [Tooltip("Перетащи сюда объект Player из иерархии")]
    public Transform player;

    [Header("Настройки времени (в секундах)")]
    public float minDelay = 2f;
    public float maxDelay = 6f;

    private float timer = 0f;
    private float nextSqueakTime;
    private bool wasInMouseRoom = false;

    // Инстанс для управления снапшотом (чтобы мы могли его включать и выключать)
    private FMOD.Studio.EventInstance snapshotInstance;

    void Start()
    {
        SetRandomTime();

        // Создаем инстанс снапшота при старте игры, если он назначен в инспекторе
        if (!mouseRoomSnapshot.IsNull)
        {
            snapshotInstance = RuntimeManager.CreateInstance(mouseRoomSnapshot);
        }
    }

    void Update()
    {
        if (player == null) return;

        bool isPlayerInRoom = CheckIfPlayerOnWood8();

        if (isPlayerInRoom)
        {
            // Если в прошлом кадре игрока тут НЕ БЫЛО, значит он только что вошел
            if (!wasInMouseRoom)
            {
                // Включаем снапшот
                snapshotInstance.start();
                wasInMouseRoom = true;
            }

            // Таймер писка работает только пока мы в комнате
            timer += Time.deltaTime;
            if (timer >= nextSqueakTime)
            {
                PlaySound();
                SetRandomTime();
                timer = 0f;
            }
        }
        else
        {
            // Если игрок только что вышел из комнаты
            if (wasInMouseRoom)
            {
                // Выключаем снапшот с плавным затуханием (ALLOWFADEOUT), если оно настроено в FMOD
                snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                UnloadMemory();

                wasInMouseRoom = false;
                timer = 0f;
            }
        }
    }

    private bool CheckIfPlayerOnWood8()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, 2.0f))
        {
            string floorName = hit.collider.gameObject.name;
            if (floorName == "Wood_8" || floorName == "Wood 8")
            {
                return true;
            }
        }
        return false;
    }

    private void SetRandomTime()
    {
        nextSqueakTime = Random.Range(minDelay, maxDelay);
    }

    private void PlaySound()
    {
        if (!squeakEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(squeakEvent, transform.position);
        }
    }

    private void UnloadMemory()
    {
        if (!squeakEvent.IsNull)
        {
            RuntimeManager.GetEventDescription(squeakEvent).unloadSampleData();
            Debug.Log("Игрок ушел с Wood_8. Снапшот выключен, память очищена!");
        }
    }

    // Очень важная функция: очищаем инстанс снапшота при удалении объекта или закрытии игры, 
    // чтобы не было утечек памяти.
    void OnDestroy()
    {
        snapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        snapshotInstance.release();
    }
}