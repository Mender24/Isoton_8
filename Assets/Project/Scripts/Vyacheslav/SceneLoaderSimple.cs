using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderSimple : MonoBehaviour
{
       public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
