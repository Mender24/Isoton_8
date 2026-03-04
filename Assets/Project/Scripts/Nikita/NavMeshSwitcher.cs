using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshSwitcher : MonoBehaviour
{
    [Header("NavMesh Surface Settings")]
    public NavMeshSurface[] navMeshSurfaces; // Массив поверхностей для переключения
    public int currentSurfaceIndex = 0; // Индекс активной поверхности

    [Header("Optional References")]
    public NavMeshAgent[] agentsToUpdate; // Агенты, которые нужно обновить после смены

    void Start()
    {
        // Деактивируем все поверхности кроме текущей
        UpdateNavMeshSurfaces();
    }

    // Метод для вызова из других скриптов или через UI кнопку
    public void SwitchToNextNavMesh()
    {
        currentSurfaceIndex = (currentSurfaceIndex + 1) % navMeshSurfaces.Length;
        UpdateNavMeshSurfaces();
    }

    // Метод для переключения на конкретную поверхность
    public void SwitchToNavMesh(int index)
    {
        if (index >= 0 && index < navMeshSurfaces.Length)
        {
            currentSurfaceIndex = index;
            UpdateNavMeshSurfaces();
        }
        else
        {
            Debug.LogWarning($"Index {index} out of range. Available surfaces: {navMeshSurfaces.Length}");
        }
    }

    private void UpdateNavMeshSurfaces()
    {
        // Деактивируем все поверхности
        foreach (var surface in navMeshSurfaces)
        {
            if (surface != null)
            {
                surface.enabled = false;
            }
        }

        // Активируем выбранную поверхность
        if (navMeshSurfaces[currentSurfaceIndex] != null)
        {
            navMeshSurfaces[currentSurfaceIndex].enabled = true;
            navMeshSurfaces[currentSurfaceIndex].BuildNavMesh(); // Перестраиваем NavMesh

            Debug.Log($"Switched to NavMesh Surface: {navMeshSurfaces[currentSurfaceIndex].name}");

            // Обновляем пути для агентов
            UpdateAgents();
        }
    }

    private void UpdateAgents()
    {
        if (agentsToUpdate != null && agentsToUpdate.Length > 0)
        {
            foreach (var agent in agentsToUpdate)
            {
                if (agent != null && agent.isActiveAndEnabled && agent.hasPath)
                {
                    // Сохраняем текущую цель и перезапускаем путь
                    Vector3 destination = agent.destination;
                    agent.ResetPath();
                    agent.SetDestination(destination);
                }
            }
        }
    }
}