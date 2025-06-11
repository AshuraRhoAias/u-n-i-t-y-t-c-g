using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CardFetcher : MonoBehaviour
{
    public string apiUrl = "http://localhost:3000/cards";
    public GameObject cardPrefab;
    public Transform cardContainer;

    [Header("UI Settings")]
    public Vector2 cardSize = new Vector2(200f, 300f);
    public float spacing = 20f;
    public int cardsPerRow = 3;

    public void StartFetching()
    {
        StartCoroutine(GetCards());
    }

    IEnumerator GetCards()
    {
        string token = PlayerPrefs.GetString("jwtToken");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("JWT token no encontrado.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta JSON: " + jsonResponse);

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    Debug.LogWarning("Respuesta vacía");
                    yield break;
                }

                ProcessCards(jsonResponse);
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode + " - " + request.error);
            }
        }
    }

    void ProcessCards(string jsonResponse)
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        try
        {
            CardListWrapper wrapper;

            if (jsonResponse.Contains("\"cards\""))
            {
                wrapper = JsonUtility.FromJson<CardListWrapper>(jsonResponse);
            }
            else if (jsonResponse.StartsWith("[") && jsonResponse.EndsWith("]"))
            {
                wrapper = JsonUtility.FromJson<CardListWrapper>(FixJson(jsonResponse));
            }
            else
            {
                wrapper = JsonUtility.FromJson<CardListWrapper>(FixJson(jsonResponse));
            }

            if (wrapper == null || wrapper.cards == null)
            {
                Debug.LogError("No se pudo deserializar el JSON.");
                return;
            }

            int index = 0;
            foreach (Card card in wrapper.cards)
            {
                int row = index / cardsPerRow;
                int col = index % cardsPerRow;

                GameObject cardObject = Instantiate(cardPrefab, cardContainer);
                RectTransform rectTransform = cardObject.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = cardSize;
                    float x = col * (cardSize.x + spacing);
                    float y = -row * (cardSize.y + spacing);
                    rectTransform.anchoredPosition = new Vector2(x, y);
                }

                SetText(cardObject, "CardName", card.name);
                SetText(cardObject, "CardDescription", card.description);
                SetText(cardObject, "CardATK", "ATK: " + card.atk);
                SetText(cardObject, "CardLevel", "LVL: " + card.level);
                SetText(cardObject, "CardLibido", "Libido: " + card.libido);

                Image cardImage = cardObject.transform.Find("CardImage")?.GetComponent<Image>();
                if (cardImage != null && !string.IsNullOrEmpty(card.image_url))
                {
                    StartCoroutine(LoadCardImage(card.image_url, cardImage));
                }

                index++;
            }

            AdjustContainerSize(wrapper.cards.Count);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error procesando JSON: " + ex.Message);
        }
    }

    void SetText(GameObject obj, string childName, string content)
    {
        Text txt = obj.transform.Find(childName)?.GetComponent<Text>();
        if (txt != null)
        {
            txt.text = content;
        }
    }

    IEnumerator LoadCardImage(string imageUrl, Image targetImage)
    {
        if (!imageUrl.StartsWith("http"))
        {
            string baseUrl = apiUrl.Substring(0, apiUrl.LastIndexOf('/'));
            imageUrl = baseUrl + "/" + imageUrl.TrimStart('/');
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            string token = PlayerPrefs.GetString("jwtToken");
            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                targetImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("No se pudo cargar la imagen: " + request.error);
            }
        }
    }

    void AdjustContainerSize(int cardCount)
    {
        RectTransform containerRect = cardContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            int rows = Mathf.CeilToInt((float)cardCount / cardsPerRow);
            float width = cardsPerRow * (cardSize.x + spacing) - spacing;
            float height = rows * (cardSize.y + spacing) - spacing;
            containerRect.sizeDelta = new Vector2(width, height);
        }
    }

    string FixJson(string value)
    {
        return "{\"cards\":" + value + "}";
    }
}

[System.Serializable]
public class Card
{
    public int id;
    public string name;
    public string description;
    public string level;
    public string libido;
    public string atk;
    public string effects;
    public string actions;
    public string image_url;
}

[System.Serializable]
public class CardListWrapper
{
    public List<Card> cards;
}
