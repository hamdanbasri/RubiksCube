using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectGroup : MonoBehaviour
{
    public int groupNumber;
    public GameObject group1;
    public GameObject group2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Selection()
    {
        if (groupNumber < 2)
        {
            groupNumber++;
        }
        else
        {
            groupNumber = 0;
        }

        if (groupNumber == 1)
        {
            group1.SetActive(true);
            group2.SetActive(false);
        }
        else if (groupNumber == 2)
        {
            group1.SetActive(false);
            group2.SetActive(true);
        }
        else
        {
            group1.SetActive(false);
            group2.SetActive(false);
        }
    }
}
