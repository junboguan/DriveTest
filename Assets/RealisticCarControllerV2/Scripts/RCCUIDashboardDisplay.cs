using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(RCCDashboardInputs))]

public class RCCUIDashboardDisplay : MonoBehaviour {

	private RCCDashboardInputs inputs;
	
	public Text RPMLabel;
	public Text KMHLabel;
	public Text GearLabel;
	
	void Start () {
		
		inputs = GetComponent<RCCDashboardInputs>();
		StartCoroutine("LateDisplay");
		
	}
	
	
	IEnumerator LateDisplay () {

		while(true){

			yield return new WaitForSeconds(.04f);
		
			RPMLabel.text = inputs.RPM.ToString("0");
			KMHLabel.text = inputs.KMH.ToString("0");
			//GearLabel.text = inputs.Gear >= 0 ? (inputs.Gear + 1).ToString("0") : "R";
            if (inputs.Gear > 0)
            {
                GearLabel.text = inputs.Gear.ToString("0");
            }
            else if (inputs.Gear == 0)
            {
                GearLabel.text = "N";
            }
            else
            {
                GearLabel.text = "R";
            }
		}

	}

}
