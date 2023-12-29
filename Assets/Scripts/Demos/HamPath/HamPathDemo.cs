using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamPathDemo : MonoBehaviour
{
	public int N;
    // Start is called before the first frame update
    void Start()
    {
        hamiltonian.Program.Main(N);
    }

 
}
