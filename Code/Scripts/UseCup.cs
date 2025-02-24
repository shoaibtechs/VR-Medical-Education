﻿using  Packages.Scene;
using  Packages.Scene.Generic;
using UnityEngine;

public class UseCup : MonoBehaviour {

    private VRTK_SnapDropZone snapdropScript;
    private Animator anim;
    private DrinkEvents drinkEventScript;
    private GameObject cupObject;
    public GameObject noClothingBody;
    // Body that can animate
    public GameObject fullClothingBody;
    public Transform snapCupPosition;
    

    private void Start()
    {
        snapdropScript = GetComponent<VRTK_SnapDropZone>();
        snapdropScript.ObjectSnappedToDropZone += new SnapDropZoneEventHandler(OnCupSnapped);
        anim = fullClothingBody.GetComponent<Animator>();
        drinkEventScript = fullClothingBody.GetComponent<DrinkEvents>();
    }

    // When cup is snapped on hand
    private void OnCupSnapped(object sender, SnapDropZoneEventArgs e)
    {
        SwitchBody();
        GetCupObject();
        Medicine med = cupObject.GetComponent<MedicineData>().medicine;
        if (!med.Equals(new Medicine())) //if the cup has a medicine attached(not the default med)
        {
            if (Tracker.medicine.Equals(med) && this.transform.parent.gameObject.GetComponent<PatientPerson>().patient.Equals(Tracker.patient)) //if correct med and correct person
            {
                Tracker.correctMedicineGiven = true;
                Tracker.quantityApplied++;
                Debug.Log("Correct medicine in cup");
            }
        }
        MoveCupToHoldPos();
        // Start drink animation
        anim.SetBool("drinkCup", true);
    }

    private void GetCupObject()
    {
        foreach (Transform child in this.transform)
        {
            if (child.tag == "waterCup")
            {
                cupObject = child.gameObject;
                drinkEventScript.cupObject = this.cupObject;
            }
        }
    }

    // Switch to animation body
    private void SwitchBody()
    {
        fullClothingBody.SetActive(true);
        noClothingBody.SetActive(false);
    }

    // Teleport cup to animation hand
    private void MoveCupToHoldPos()
    {
        cupObject.GetComponent<Rigidbody>().useGravity = false;
        cupObject.GetComponent<Rigidbody>().isKinematic = true;
		cupObject.transform.position = snapCupPosition.position;
		snapCupPosition.gameObject.GetComponent<SphereCollider> ().enabled = false;
    }
}
