using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _facilities;

    private void Awake()
    {
        if (_facilities != null)
        {
            for (int i = 0; i < _facilities.Count; i++)
            {
                _facilities[i].SetActive(true);
            }
        }
    }
}
