using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class particleScript : MonoBehaviour {

    private Vector3 mousePos;

    // GameObject(mold)
    public GameObject particle;
    private List<GameObject> objList = new List<GameObject>();
    public int maxObject = 20;
    public float screenWidth = 13.3f;
    public float screenHeight = 10f;

    //Coordinates received from Kinect
    private Vector3[] inputXY;
    private int inputCount = 0;

    Socket SeverSocket = null;
    Socket client = null;
    Thread Socket_Thread = null;
    bool Socket_Thread_Flag = false;


    private Material lineMaterial;


    void Awake()
    {
        Socket_Thread = new Thread(Dowrk);
        Socket_Thread_Flag = true;
        Socket_Thread.Start();
    }

    // Use this for initialization
    void Start () {
        StartCoroutine("CreatePositions");
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 3f;
        objList.ForEach(delegate (GameObject obj)
        {
            //=======================================================
            // 각각의 오브젝트 랜덤이동
            float ranX = UnityEngine.Random.Range(-1.0f, 1.0f);
            float ranY = UnityEngine.Random.Range(-1.0f, 1.0f);
            obj.transform.Translate(new Vector3(ranX * speed * Time.deltaTime, ranY * speed * Time.deltaTime, 0));

            //=======================================================
            // 키넥트로 받은 좌표 처리
            for (int i = 0; i<inputCount; i++)
            {
                if (Vector3.Distance(inputXY[i], obj.transform.position) < 2f) // 키넥트 좌표와 거리가 200px 이하일 경우
                {
                    Vector3 pos = Vector3.MoveTowards(obj.transform.position, inputXY[i], Time.deltaTime * speed); // 끌어당김
                    obj.transform.position = pos;
                }
            }
            //=======================================================
            // 마우스이벤트로 받은 좌표 처리
            if (Vector3.Distance(mousePos, obj.transform.position) < 2f) // 마우스 좌표와 거리가 200px 이하일 경우
            {
                // 마우스이벤트를 받을때
                if (Input.GetMouseButton(0))
                {
                    mousePos = Vector3.MoveTowards(obj.transform.position, mousePos, Time.deltaTime * speed);
                    obj.transform.position = mousePos;
                }
            }
        });

        //=======================================================
        // 마우스이벤트처리
        if (Input.GetMouseButton(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        }
    }

    //=======================================================
    // 2초마다 랜덤 오브젝트 생성, 연결 끊긴경우 서버소켓 재실행
    IEnumerator CreatePositions()
    {
        float randX = UnityEngine.Random.Range(0, 13.0f);
        float randY = UnityEngine.Random.Range(-10.0f, 0);
        if(objList.Count < maxObject)  // 최대 오브젝트 생성 갯수
        {
            GameObject mold = (GameObject)Instantiate(particle, new Vector3(randX, randY, 0f), Quaternion.identity);
            objList.Add(mold);
        }        

        if (!Socket_Thread_Flag)
        {
            Socket_Thread = new Thread(Dowrk);
            Socket_Thread_Flag = true;
            Socket_Thread.Start();
        }
        yield return new WaitForSeconds(2);// 2초마다
        StartCoroutine("CreatePositions");
    }

    void OnApplicationQuit()
    {
        try
        {
            client.Close();
            Socket_Thread_Flag = false;
            Socket_Thread.Abort();
            SeverSocket.Close();
        }

        catch
        {
            Debug.Log("소켓과 쓰레드 종료때 오류가 발생");
        }
    }


    //=======================================================
    // 서버소켓 쓰레드
    private void Dowrk()
    {
        SeverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 6777);
        SeverSocket.Bind(ipep);
        SeverSocket.Listen(10);

        Debug.Log("소켓 대기중....");
        client = SeverSocket.Accept();//client에서 수신을 요청하면 접속합니다.
        Debug.Log("소켓 연결되었습니다.");

        IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
        NetworkStream recvStm = new NetworkStream(client);



        while (Socket_Thread_Flag)
        {
            byte[] receiveBuffer = new byte[1024 * 80];
            try
            {
                int readbytes = recvStm.Read(receiveBuffer, 0, receiveBuffer.Length);

                //=======================================================
                // 클라이언트 소켓접속이 끊어진경우
                if (readbytes == 0) 
                {                    
                    Debug.Log("소켓 접속 끊어짐.");
                    Socket_Thread_Flag = false;
                    client.Close();
                    SeverSocket.Close();
                    continue;
                }

                //=======================================================
                // 키넥트로 받은 좌표 처리
                string[] recvString = Encoding.Default.GetString(receiveBuffer).Split('E');
                Debug.Log(recvString);
                inputXY = new Vector3[50];
                for (int i = 0; i < recvString.Length - 1; i++)
                {
                    string[] xylocation = recvString[i].Split(','); // 받는 좌표값  = X , Y 가 0~1 사이의 화면에 있는 비율값임 ex> 정가운데 = 0.5,0.5
                    inputXY[i] = new Vector3(System.Convert.ToSingle(xylocation[0]) * screenWidth, -(System.Convert.ToSingle(xylocation[1]) * screenHeight), 0f); // y값은 -를 해줘야함
                    inputCount = i + 1;
                    Debug.Log("REC-INPUT ["+ i +"] ( X , Y ) = ( " + inputXY[i].x + " , " + inputXY[i].y + " )");
                    Debug.Log("input Count = " + inputCount);
                }             
            }

            catch (Exception e)
            {
                Debug.Log(e);
                Socket_Thread_Flag = false;
                client.Close();
                SeverSocket.Close();
                continue;
            }

        }
        

    }



}
