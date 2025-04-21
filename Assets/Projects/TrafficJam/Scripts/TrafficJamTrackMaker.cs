using UnityEngine;

public class TrafficJamTrackMaker : MonoBehaviour
{
    [Header("Input")]
    public Texture2D inputImage;

    [Header("Road Prefabs (used for white pixels)")]
    public GameObject straightVerticalPrefab;
    public GameObject straightHorizontalPrefab;
    public GameObject cornerNEPrefab;
    public GameObject cornerNWPrefab;
    public GameObject cornerSEPrefab;
    public GameObject cornerSWPrefab;

    [Header("Ground Prefab (used for black pixels)")]
    public GameObject groundPrefab;

    [Header("Grid Settings")]
    public float pixelSpacing = 1.0f;
    public bool centerGrid = true;

    [Header("Threshold Settings")]
    [Range(0f, 1f)]
    public float placementThreshold = 0.9f;

    [ContextMenu("Generate From Image")]
    public void Generate(Vector3 environmentOffset)
    {
        if (inputImage == null)
        {
            Debug.LogError("Missing input image.");
            return;
        }

        int width = inputImage.width;
        int height = inputImage.height;

        Vector3 offset = centerGrid
            ? new Vector3(-width / 2f * pixelSpacing, 0f, -height / 2f * pixelSpacing)
              + new Vector3(pixelSpacing / 2f, 0f, pixelSpacing / 2f)
            : Vector3.zero;

        offset += environmentOffset; // Apply environment offset

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixel = inputImage.GetPixel(x, y);
                Vector3 position = new Vector3(
                    x * pixelSpacing,
                    0f,
                    y * pixelSpacing
                ) + offset;

                if (IsWhite(pixel))
                {
                    bool n = IsWhite(GetPixelSafe(x, y + 1));
                    bool s = IsWhite(GetPixelSafe(x, y - 1));
                    bool e = IsWhite(GetPixelSafe(x + 1, y));
                    bool w = IsWhite(GetPixelSafe(x - 1, y));

                    GameObject roadPrefab = null;

                    // Exclusive matching for 2-connections
                    if (n && s && !e && !w) roadPrefab = straightVerticalPrefab;
                    else if (e && w && !n && !s) roadPrefab = straightHorizontalPrefab;
                    else if (n && e && !s && !w) roadPrefab = cornerNEPrefab;
                    else if (n && w && !s && !e) roadPrefab = cornerNWPrefab;
                    else if (s && e && !n && !w) roadPrefab = cornerSEPrefab;
                    else if (s && w && !n && !e) roadPrefab = cornerSWPrefab;

                    if (roadPrefab != null)
                        Instantiate(roadPrefab, position, Quaternion.identity, this.transform);
                    else
                        Debug.LogWarning($"Unmatched road config at ({x},{y}) — N:{n} S:{s} E:{e} W:{w}");
                }
                else if (IsBlack(pixel))
                {
                    if (groundPrefab != null)
                        Instantiate(groundPrefab, position, Quaternion.identity, this.transform);
                }
            }
        }

    }

    private Color GetPixelSafe(int x, int y)
    {
        if (x < 0 || x >= inputImage.width || y < 0 || y >= inputImage.height)
            return Color.black;
        return inputImage.GetPixel(x, y);
    }

    private bool IsWhite(Color pixel)
    {
        return pixel.grayscale >= placementThreshold;
    }

    private bool IsBlack(Color pixel)
    {
        return pixel.grayscale < placementThreshold;
    }
}
