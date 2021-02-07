using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Parametre : MonoBehaviour
{
    [SerializeField] string _titre;
    [SerializeField] TMPro.TMP_Text _label;
    [SerializeField] TMPro.TMP_Text _valeur_affichee;
    [SerializeField] string _valeur_affichee_format;
    [SerializeField] UnityEngine.UI.Slider _slider;

    public float _valeur_min;
    public float _valeur_max;
    public float _valeur;

    private void Start()
    {
        _label.text = _titre;

        _slider.minValue = _valeur_min;
        _slider.maxValue = _valeur_max;
        _slider.value = _valeur;
    }

    private void Update()
    {
        _valeur_affichee.text = _slider.value.ToString(_valeur_affichee_format);
    }

    public void _SetValue(float val)
    {
        _valeur = val;
    }
    public void _SetMinValue(float val)
    {
        _slider.minValue = val;
    }
    public void _SetMaxValue(float val)
    {
        _slider.maxValue = val;
    }
}
