using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBeatResponse : MonoBehaviour
{
    [SerializeField] bool _testBeat;
    [SerializeField] float _pulseSize = 1.5f;   // How much the square increases in size by
    [SerializeField] float _returnSpeed = 3f;   // How fast it returns back

    [SerializeField] BeatManager _beatManager;

    private Vector3 _startSize;
    private bool _isOnBeat;
    private SpriteRenderer _SpriteRenderer;
    private bool _input;

    void Start(){
        _startSize = transform.localScale;
        _SpriteRenderer = GetComponent<SpriteRenderer>();
        if (_testBeat){
            StartCoroutine(TestBeat());
        }
    }

    // Update is called once per frame
    void Update(){
        CheckTiming();
        transform.localScale = Vector3.Lerp(transform.localScale, _startSize, Time.deltaTime * _returnSpeed);
        _SpriteRenderer.color = Color.Lerp(_SpriteRenderer.color, Color.white, Time.deltaTime * _returnSpeed);

    }

    public void Pulse(){
        transform.localScale = _startSize * _pulseSize; 
    }

    private void CheckTiming(){
        _input = Input.GetKeyDown("p");
        if (_input){
            _isOnBeat = _beatManager.IsOnBeat();
            DemoTiming();
        }
    }

    private void DemoTiming(){
        if (_isOnBeat){
            _SpriteRenderer.color = Color.green;
        }else{
            _SpriteRenderer.color = Color.red;
        }
    }

    IEnumerator TestBeat(){
        while (true){
            yield return new WaitForSeconds(60f / 138f);
            Pulse();
        }
    }
}
