using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class CourseChapter
{
    public string CourseChapterTitle;
    public List<CourseSection> CourseSection;
    public List<GameObject> CourseSectionCards;
}

[System.Serializable]
public class CourseSection
{
    public string CourseSectionTitle;
}

public class CourseContentManager : MonoBehaviour
{
    public Transform Container;
    public GameObject ChapterCardPrefab;
    public GameObject SectionCardPrefab;

    public List<CourseChapter> m_courseChapters = new List<CourseChapter>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i =0; i<m_courseChapters.Count; i++)
        {
            GameObject ChapterCard = Instantiate(ChapterCardPrefab, Container);
            ChapterCard.GetComponent<ChapterCardReference>().ChapterTitle.text = $"Chapter {i+1} : {m_courseChapters[i].CourseChapterTitle}";
            //button == arg entry on click toggleCard(i)

            for(int j=0; j < m_courseChapters[i].CourseSection.Count; j++)
            {
                GameObject Sectioncard = Instantiate(SectionCardPrefab, Container);
                Sectioncard.GetComponent<SectionCardReference>().SectionTitle.text = $"Section {j+1} : {m_courseChapters[i].CourseSection[j].CourseSectionTitle}";
                m_courseChapters[i].CourseSectionCards.Add(Sectioncard);
            }
        }        
    }

    public void toggleCard(int chatercard)
    {
        foreach (GameObject item in m_courseChapters[chatercard].CourseSectionCards)
        {
            if (item.activeSelf)
            {
                item.SetActive(false);
            }
            else
            {
                item.SetActive(true);
            }
        }
    }
}
