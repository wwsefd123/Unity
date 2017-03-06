using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class particleScript : MonoBehaviour {

    public GameObject particle;
    public List<GameObject> objList = new List<GameObject>();
    //private Vector3 mousePos;

    private Vector3[] inputXY;
    private int inputCount = 0;

    Socket SeverSocket = null;
    Socket client = null;
    Thread Socket_Thread = null;
    bool Socket_Thread_Flag = false;


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
            float ranX = UnityEngine.Random.Range(-1.0f, 1.0f);
            float ranY = UnityEngine.Random.Range(-1.0f, 1.0f);
            obj.transform.Translate(new Vector3(ranX * speed * Time.deltaTime, ranY * speed * Time.deltaTime, 0));

            for(int i = 0; i<inputCount; i++)
            {
                if (Vector3.Distance(inputXY[i], obj.transform.position) < 2f)
                {
                    Vector3 pos = Vector3.MoveTowards(obj.transform.position, inputXY[i], Time.deltaTime * speed);
                    obj.transform.position = pos;
                }
            }
            //if (Vector3.Distance(new Vector3(inputX[0], inputY[0], 0f), obj.transform.position) < 2f)
            //{
            //    Vector3 pos = Vector3.MoveTowards(obj.transform.position, new Vector3(inputX[0], inputY[0], 0f), Time.deltaTime * speed);
            //    obj.transform.position = pos;
            //}

            //else if(Vector3.Distance(mousePos, obj.transform.position) < 2f)
            //{
            //    // 마우스이벤트를 받을때
            //    if (Input.GetMouseButton(0))
            //    {
            //        mousePos = Vector3.MoveTowards(obj.transform.position, mousePos, Time.deltaTime * speed);
            //        obj.transform.position = mousePos;
            //    }
            //}            
        });
        //if (Input.GetMouseButton(0))
        //{
        //    mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        //}
    }

    IEnumerator CreatePositions()
    {
        float randX = UnityEngine.Random.Range(0, 13.0f);
        float randY = UnityEngine.Random.Range(-10.0f, 0);
        //if(objList.Count < 20)
        //{
            GameObject enemy = (GameObject)Instantiate(particle, new Vector3(randX, randY, 0f), Quaternion.identity);
            objList.Add(enemy);
        //}        
        yield return new WaitForSeconds(2);
        StartCoroutine("CreatePositions");
    }

    void OnApplicationQuit()
    {
        try
        {
            client.Close();
            SeverSocket.Close();
            Socket_Thread_Flag = false;
            Socket_Thread.Abort();
            SeverSocket.Close();
        }

        catch
        {
            Debug.Log("소켓과 쓰레드 종료때 오류가 발생");
        }
    }
    
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
                recvStm.Read(receiveBuffer, 0, receiveBuffer.Length);
                string[] Test = Encoding.Default.GetString(receiveBuffer).Split('E');
                Debug.Log(Test);
                inputXY = new Vector3[50];
                for (int i = 0; i < Test.Length - 1; i++)
                {
                    string[] xylocation = Test[i].Split(',');
                    inputXY[i] = new Vector3(System.Convert.ToSingle(xylocation[0]) * 13f, -(System.Convert.ToSingle(xylocation[1]) * 10f), 0f);
                    //inputXY[i] = new Vector3(13f - System.Convert.ToSingle(xylocation[0]) / 486 * 13f, -(10f - System.Convert.ToSingle(xylocation[1]) / 1800 * 10f), 0f);
                    inputCount = i + 1;
                    Debug.Log("REC-INPUT ["+ i +"] ( X , Y ) = ( " + inputXY[i].x + " , " + inputXY[i].y + " )");
                    Debug.Log("input Count = " + inputCount);
                }             
            }

            catch (Exception e)
            {
                Socket_Thread_Flag = false;
                client.Close();
                SeverSocket.Close();
                continue;
            }

        }

    }




}
