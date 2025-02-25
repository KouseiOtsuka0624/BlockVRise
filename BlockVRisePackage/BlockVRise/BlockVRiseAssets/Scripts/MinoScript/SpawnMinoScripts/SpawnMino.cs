using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMino : MonoBehaviour
{
    //Start is called before the first frame update
    [SerializeField] private GameObject _leftController;
    private GameObject _handMino;
    [SerializeField] private GameObject[] _minos;
    [SerializeField] private GameObject[] _handMinos;
    private int _minoSpawnOrderNum;
    void Start()
    {
        SpawnInitialMino();
        SpawnHandMino();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnInitialMino()
    {
        Instantiate(_minos[Random.Range(0, _minos.Length)] , transform.position, Quaternion.identity);
    }
    private void SpawnHandMino()
    {
        Destroy(_handMino);
        _minoSpawnOrderNum = Random.Range(0, _minos.Length);

        _handMino=Instantiate(_handMinos[_minoSpawnOrderNum] , _leftController.transform.position, Quaternion.identity);
        _handMino.transform.parent = _leftController.transform;
        _handMino.transform.localPosition = Vector3.zero;
    }
    public void SpawnNewMino()
    {
        Instantiate(_minos[_minoSpawnOrderNum] , transform.position, Quaternion.identity);
        SpawnHandMino();
    }
}