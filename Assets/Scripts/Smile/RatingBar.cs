using UnityEngine;
using UnityEngine.UI;

public class RatingBar : MonoBehaviour
{
    public Image[] stars; // Assign the star images in the inspector
    public Sprite emptyStar; // The sprite for an empty star
    public Sprite filledStar; // The sprite for a filled star

    private int currentRating = 0;

    void Start()
    {
        // Initialize with zero rating
        UpdateRating(0);
    }

    public void UpdateRating(int rating)
    {
        currentRating = rating;

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < currentRating)
            {
                stars[i].sprite = filledStar;
            }
            else
            {
                stars[i].sprite = emptyStar;
            }
        }
    }

    // Example method to set rating, you can call this method from UI buttons or other interactions
    public void SetRating(int rating)
    {
        if (rating >= 0 && rating <= stars.Length)
        {
            UpdateRating(rating);
        }
    }
}
