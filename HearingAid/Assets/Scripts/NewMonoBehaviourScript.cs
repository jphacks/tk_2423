using UnityEngine;
using System.Collections;

public class SpectramLineRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float waveLength = 20.0f;
    [SerializeField] private float yLength = 10f;

    private AudioSource source = null;
    
    private Vector3[] points = null;
    private const int FFT_RESOLUTION = 2048;
    private float[] spectram = new float[FFT_RESOLUTION];

    private void Start()
    {
        source = GetComponent<AudioSource>();
        var clip = source.clip;
        var data = new float[clip.channels * clip.samples];
        source.clip.GetData(data, 0);

        Prepare(source, data);
    }

    public void Prepare(AudioSource source, float[] data)
    {
        this.source = source;
        this.spectram = new float[FFT_RESOLUTION];
        this.points = new Vector3[FFT_RESOLUTION];
    }

    public void FixedUpdate()
    {
        Render();
    }

    private void Render()
    {
        source.GetSpectrumData(spectram, 0, FFTWindow.BlackmanHarris);
        var xStart = -waveLength / 2;
        var xStep = waveLength / spectram.Length;
        for (var i = 0; i < points.Length; i++)
        {
            var y = spectram[i] * yLength;
            var x = xStart + xStep * i;
            var p = new Vector3(x, y, 0) + transform.position;
            points[i] = p;
        }

        Render(points);
    }

    private void Render(Vector3[] points)
    {
        if (points == null) return;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private void Reset()
    {
        var x = waveLength / 2;
        Render(new[]
        {
            new Vector3(-x, 0, 0) + transform.position,
            new Vector3(0, 0, 0) + transform.position,
            new Vector3(x, 0, 0) + transform.position,
        });
    }
}

