using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

namespace ChaosMod;

public class AdManager : MonoBehaviour
{
    private TextMeshProUGUI ADText;
    private Button SkipButton;
    private TextMeshProUGUI SkipText;
    private VideoPlayer video;
    private GameObject playerImage;
    public List<VideoClip> clips;

    private bool spawned = false;
    private float skipTimer = 6f;
    private bool skipPressed = false;

    void SetupComponents()
    {
        if (video == null)
            video = GetComponent<VideoPlayer>();
        Transform[] shits = GetComponentsInChildren<Transform>();
        foreach (Transform _t in shits)
        {
            switch (_t.name)
            {
                case "Skip Button": {
                    SkipButton = _t.GetComponent<Button>();
                    break;
                }
                case "Skip Text": {
                    SkipText = _t.GetComponent<TextMeshProUGUI>();
                    break;
                }
                case "Tab Text": {
                    ADText = _t.GetComponent<TextMeshProUGUI>();
                    break;
                }
                case "Video": {
                    playerImage = _t.gameObject;
                    break;
                }
            }
        }
    }

    void Start()
    {
        SetupComponents();

        ADText.text = "AD";
        SkipButton.onClick.AddListener(delegate()
        {
            if (skipTimer <= 0f)
                skipPressed = true;
        });
        playerImage.SetActive(false);
        SkipButton.gameObject.SetActive(false);
        video.Stop();
    }

    public void SetVideo()
    {
        video.Stop();
        video.clip = clips[UnityEngine.Random.Range(0, clips.Count)];
        video.Prepare();
        video.Stop();
    }

    public float GetLength()
    {
        return (float)video.length;
    }

    [ContextMenu("보이기")]
    public void Show()
    {
        StartCoroutine(InAnimation());
    }

    [ContextMenu("숨기기")]
    public void Hide()
    {
        StartCoroutine(OutAnimation());
    }

    IEnumerator InAnimation()
    {
        float time = 0f;
        Func<float, float> ease = x =>
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
        };

        float startX = 1230f;
        float endX = 690f;
        float ogY = transform.localPosition.y;

        while (time < 1f)
        {
            float per = ease(time);
            transform.localPosition = new Vector3(startX + (per * (endX - startX)), ogY, 0);

            time += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = new Vector3(endX, ogY, 0);
        playerImage.SetActive(true);
        video.Play();
        SkipButton.gameObject.SetActive(true);
        spawned = true;
        skipTimer = 6f;
        SkipText.text = "Video Ends\nafter 5 seconds";
        skipPressed = false;
    }

    IEnumerator OutAnimation()
    {
        float time = 0f;
        Func<float, float> ease = x => x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);

        float startX = 690f;
        float endX = 1230f;
        float ogY = transform.localPosition.y;
        video.Stop();
        playerImage.SetActive(false);

        while (time < 1f)
        {
            float per = ease(time);
            transform.localPosition = new Vector3(Mathf.Lerp(startX, endX, per), ogY, 0);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = new Vector3(endX, ogY, 0);
        SkipButton.gameObject.SetActive(false);
        spawned = false;
    }

    void Update()
    {
        if (spawned)
        {
            if (skipTimer > 0f)
                skipTimer -= Time.unscaledDeltaTime;
            skipTimer = Mathf.Max(skipTimer, 0f);
            if (skipTimer <= 0f)
            {
                SkipText.text = skipPressed ? "no you cant" : "Skip AD";
                return;
            }
            SkipText.text = $"Video Ends\nafter {Mathf.FloorToInt(skipTimer)} seconds";   
        }
    }
}
