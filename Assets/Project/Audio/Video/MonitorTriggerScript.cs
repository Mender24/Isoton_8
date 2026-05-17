using System.Collections;
using UnityEngine;
using UnityEngine.Video;
public class MonitorTriggerScript : MonoBehaviour
{
    
    [SerializeField] private VideoPlayer tvVideoPlayer;
    [SerializeField] private AudioSource tvAudioSource;
    [SerializeField] private MeshRenderer tvColorPlane;
    [SerializeField] private float _workTime;
    private void Start()
    {
        tvColorPlane.material.color = Color.black;

        tvVideoPlayer.Stop();
        tvAudioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tvColorPlane.material.color = Color.white;

            tvVideoPlayer.Play();
            tvAudioSource.Play();

          
            GetComponent<Collider>().enabled = false;

            StartCoroutine(OffTv());
        }
    }

    private IEnumerator OffTv()
    {
        yield return new WaitForSeconds(_workTime);

        tvColorPlane.material.color = Color.black;
        tvVideoPlayer.Stop();
        tvAudioSource.Stop();
    }
}
