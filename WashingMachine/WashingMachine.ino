/*============================================================================================================================================================*/
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
//  引用内容
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/*============================================================================================================================================================*/
#include <SCoop.h>
#include <U8g2lib.h>
#include <Arduino.h>
#ifdef U8X8_HAVE_HW_SPI
#include <SPI.h>
#endif
#ifdef U8X8_HAVE_HW_I2C
#include <Wire.h>
#endif

/*============================================================================================================================================================*/
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
//  变量定义
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/*============================================================================================================================================================*/
/*洗衣状态*/
int washingStatus = 0;  //洗衣状态
int tempStatus = 0;     //上一个状态
int actionTrigger = 0;  //动作监听
int pageCache;  //页面暂存


/*
  0：待机
  1：正在进水
  2：进水完成
  3：正在洗衣
  4：洗衣完成
  5：正在排水
  6：排水完成
  7：正在甩干
  8：甩干完成
  9：洗衣完成
*/

/*水位监测信号*/
int WaterIn = 3;    //进水信号
int WaterOut = 4;   //排水信号

/*水阀开关*/
int Invalve = 12; //进水阀
int Outvalve = 13;  //出水阀

/*电机信号*/
int MotorReg = 5; //电机正转
int MotorRes = 6; //电机反转

/*按钮编码功能变量*/
int OrderButtonSignal1 = 9;    //按钮信号1位
int OrderButtonSignal2 = 10;    //按钮信号2位
int OrderButtonSignal3 = 11;    //按钮信号3位

/*菜单功能变量*/
int ListCount;              /*菜单选项数*/
int ListPageIndex = 1;      /*菜单页码*/
int OrderIndex = 1;         /*选项标记位置*/
char *TitleText;            /*标题名*/
char *item1Text;            /*选项1*/
char *item2Text;            /*选项2*/
char *item3Text;            /*选项3*/

/*矩阵键盘编码防抖动变量*/
int SigBefore = 0;

/*定义OLED屏幕*/
U8G2_SSD1306_128X64_NONAME_1_HW_I2C u8g2(U8G2_R0, /* clock=*/ SCL, /* data=*/ SDA, /* reset=*/ U8X8_PIN_NONE);

/*串口监听*/
String SerialRead = "";
int logLen = 0;

/*多线程定义*/
defineTask( Action );    //事务处理进程
defineTask( Listener );  //监听进程
defineTask( ComSerial );   //串口通讯进程

/*============================================================================================================================================================*/
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
//  函数定义
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/*============================================================================================================================================================*/
//** I.Action 部分-------------------------------------------
//  1.基础功能
//进水
void InputWater() {
  washingStatus = 1;
  sleep(1000);
  digitalWrite(Invalve, HIGH);
  while (digitalRead(WaterIn) != HIGH) {}
  digitalWrite(Invalve, LOW);
  washingStatus = 2;
  sleep(1000);
}

//排水
void OutputWater() {
  washingStatus = 5;
  sleep(1000);
  digitalWrite(Outvalve, HIGH);
  while (digitalRead(WaterOut) != HIGH) {}
  washingStatus = 6;
  sleep(1000);
}

//洗衣作业电机转动
void Wash_MotorWork(int level) {
  washingStatus = 3;
  sleep(1000);
  int duration, cycle = 3;
  switch (level) {
    case 1:
      duration = 3000;
      break;
    case 2:
      duration = 4000;
      break;
    case 3:
      duration = 5000;
      break;
    default:
      duration = 3000;
      break;
  }
  for (int i = 0; i < cycle; i++) {
    digitalWrite(MotorReg, HIGH);
    sleep(2000);
    digitalWrite(MotorReg, LOW);
    sleep(1000);
    digitalWrite(MotorRes, HIGH);
    sleep(2000);
    digitalWrite(MotorRes, LOW);
    sleep(1000);
  }
  washingStatus = 4;
  sleep(1000);
}

//甩干作业洗衣机转动
void Dry_MotorWork(int time) {
  washingStatus = 7;
  sleep(1000);
  digitalWrite(Outvalve, HIGH);
  digitalWrite(MotorReg, HIGH);
  sleep(time * 10000);
  digitalWrite(Outvalve, LOW);
  digitalWrite(MotorReg, LOW);
  washingStatus = 8;
  sleep(1000);
}

//2.功能整合
//自动洗衣+甩干
void wash(int level) {
  InputWater();
  Wash_MotorWork(level);
  OutputWater();
  Dry_MotorWork(1);
  washingStatus = 9;
}

//甩干
void dry(int time) {
  Dry_MotorWork(time);
  washingStatus = 9;
}

//** II.Listener 部分-------------------------------------------
//  1.基础功能
//居中对齐显示
void centrl(int top, char *str) {
  int left = (128 - u8g2.getStrWidth(str)) / 2;
  u8g2.drawStr(left, top, str);
}

//暂停洗衣
void StopMotor() {
  digitalWrite(MotorReg, LOW);
  digitalWrite(MotorRes, LOW);
}

//绘制显示内容
void setOrder(int chosed) {
  /*===================显示菜单====================
     输入格式：
        chosed  ——  选项选择记号位置
     Order 构造：
        菜单标题
            选项1
            选项2
            选项3
     Order菜单只支持显示3栏选项
     =============================================*/
  u8g2.firstPage();
  do {
    u8g2.setFont(u8g2_font_ncenB14_tr);
    int TitleItemHeight = u8g2.getAscent() - u8g2.getDescent();
    u8g2.setFont(u8g_font_unifont);
    int ListItemHeight = u8g2.getAscent() - u8g2.getDescent();
    //int ListItemHeight = 14;
    u8g2.drawFrame(0, 12, 128, 48);
    u8g2.setFont(u8g2_font_ncenB14_tr);
    centrl(TitleItemHeight, TitleText);
    u8g2.setFont(u8g_font_unifont);
    if (item1Text != "") centrl(ListItemHeight * 1 + TitleItemHeight, item1Text);
    if (item2Text != "") centrl(ListItemHeight * 2 + TitleItemHeight, item2Text);
    if (item3Text != "") centrl(ListItemHeight * 3 + TitleItemHeight, item3Text);
    /*绘制选择记号*/
    u8g2.drawTriangle(10, ListItemHeight * chosed + TitleItemHeight, 10, ListItemHeight * (chosed - 1) + TitleItemHeight, 10 + ListItemHeight * 0.7, ListItemHeight * (chosed - 0.5) + TitleItemHeight);
  } while (u8g2.nextPage());
}

//菜单内容控制
void SetupOrderList(int Page) {
  /* ==============菜单控制逻辑============
     父级菜单 * 10 + OrderIndex = 目标菜单
     当前菜单 / 10 = 父级菜单 */
  switch (Page) {
    /*    主页     */
    case 1:
      TitleText = "Home";
      item1Text = "Wash";
      item2Text = "Dry";
      item3Text = "";
      ListCount = 2;
      break;
    /*    洗衣     */
    case 11:
      TitleText = "Wash Cloth";
      item1Text = "Normal";
      item2Text = "Silent";
      item3Text = "Quick";
      ListCount = 3;
      break;
    /*    甩干     */
    case 12:
      TitleText = "Dry";
      item1Text = "2min";
      item2Text = "3min";
      item3Text = "5min";
      ListCount = 3;
      break;
    /*    洗衣中     */
    case 110:
      TitleText = "Washing";
      item1Text = "Pause";
      item2Text = "Terminate";
      item3Text = "";
      ListCount = 2;
      break;
    /*    甩干中     */
    case 120:
      TitleText = "Drying";
      item1Text = "Pause";
      item2Text = "Terminate";
      item3Text = "";
      ListCount = 2;
      break;
    /*    暂停     */
    case 2:
      TitleText = "Paused";
      item1Text = "Resume";
      item2Text = "Terminate";
      item3Text = "";
      ListCount = 2;
      break;
    /*    完成洗衣     */
    case 3:
      TitleText = "";
      item1Text = "Done!";
      item2Text = "";
      item3Text = "";
      ListCount = 1;
      break;
    default:
      break;
  }
}

//执行菜单任务
void DoFunction(int Index) {
  switch (Index) {
    /*洗衣部分*/
    case 111:
    case 112:
    case 113:
      actionTrigger = Index - 110;
      break;
    /*甩干部分*/
    case 121:
    case 122:
    case 123:
      actionTrigger = Index - 117;
      break;
    default: break;
  }
}

//执行进行中任务
void WorkingFunction(int Index) {
  switch (Index) {
    /*暂停*/
    case 1:
      pageCache = ListPageIndex;
      Action.pause();
      StopMotor();
      ListPageIndex = 2;
      OrderIndex = 1;
      SetupOrderList(ListPageIndex);
      setOrder(OrderIndex);
      break;
    /*终止*/
    case 2:
      Action.pause();
      StopMotor();
      ListPageIndex = 1;
      OrderIndex = 1;
      SetupOrderList(ListPageIndex);
      setOrder(OrderIndex);
      break;
    /*继续*/
    case 3:
      Action.resume();
      if (washingStatus == 7) digitalWrite(MotorReg, HIGH);
      ListPageIndex = pageCache;
      OrderIndex = 1;
      SetupOrderList(ListPageIndex);
      setOrder(OrderIndex);
      break;
    /*完成洗衣*/
    case 9:
      ListPageIndex = 3;
      OrderIndex = 1;
      SetupOrderList(ListPageIndex);
      setOrder(OrderIndex);
    default:
      break;
  }
}

//矩阵键盘编码
void Decode() {
  int Sig1 = digitalRead(OrderButtonSignal1);
  int Sig2 = digitalRead(OrderButtonSignal2);
  int Sig3 = digitalRead(OrderButtonSignal3);
  int Sig = (Sig1 == HIGH ? 0 : 1) * 4 + (Sig2 == HIGH ? 0 : 1) * 2 + (Sig3 == HIGH ? 0 : 1) * 1; /*信号编码转为十进制*/
  if (SigBefore != Sig) {   /*矩阵键盘编码防抖动*/
    OrderButtonFunction(Sig);
  }
  delay(50);
  SigBefore = Sig;
}

//矩阵编码控制
void OrderButtonFunction(int Signal) {
  if (washingStatus == 0) {
    switch (Signal) {
      /*编码 000  不做处理*/
      case 0:
        break;
      /*编码 001  选项标记下移*/
      case 1:
        if (OrderIndex < ListCount) {
          setOrder(++OrderIndex);
        }
        else {
          OrderIndex = 1;
          setOrder(OrderIndex);
        }
        break;
      /*编码 002  选项标记上移*/
      case 2:
        if (OrderIndex > 1) {
          setOrder(--OrderIndex);
        }
        else {
          OrderIndex = ListCount;
          setOrder(OrderIndex);
        }
        break;
      /*编码 003  选项确定*/
      case 3:
        if (ListPageIndex / 10 == 1) {
          DoFunction(ListPageIndex * 10 + OrderIndex);
          ListPageIndex = ListPageIndex * 10;
          OrderIndex = 1;
          SetupOrderList(ListPageIndex);
          setOrder(OrderIndex);
        }
        else {
          ListPageIndex = ListPageIndex * 10 + OrderIndex;
          OrderIndex = 1;
          SetupOrderList(ListPageIndex);
          setOrder(OrderIndex);
        }
        break;
      /*编码 004  选项返回*/
      case 4:
        if (ListPageIndex != 1) {
          ListPageIndex /= 10;
          OrderIndex = 1;
          SetupOrderList(ListPageIndex);
          setOrder(OrderIndex);
        }
        break;
      /*编码 005  返回主页*/
      case 5:
        ListPageIndex = 1;
        OrderIndex = 1;
        SetupOrderList(ListPageIndex);
        setOrder(OrderIndex);
        break;
      /*编码 006  未定义*/
      case 6:
        break;
      /*编码 007  未定义*/
      case 7:
        break;
      default:
        break;
    }
  }
  /*完成洗衣返回确认*/
  else if (washingStatus == 9) {
    switch (Signal) {
      case 3:
        washingStatus = 0;
        actionTrigger = 0;
        ListPageIndex = 1;
        OrderIndex = 1;
        SetupOrderList(ListPageIndex);
        setOrder(OrderIndex);
        break;
      default:
        break;
    }
  }
  else {
    switch (Signal) {
      case 1:
        if (OrderIndex < ListCount) {
          setOrder(++OrderIndex);
        }
        else {
          OrderIndex = 1;
          setOrder(OrderIndex);
        }
        break;
      /*编码 002  选项标记上移*/
      case 2:
        if (OrderIndex > 1) {
          setOrder(--OrderIndex);
        }
        else {
          OrderIndex = ListCount;
          setOrder(OrderIndex);
        }
        break;
      /*编码 003  选项确定*/
      case 3:

        if (ListPageIndex == 2) WorkingFunction(3);
        else WorkingFunction(OrderIndex);
        break;
      default:
        break;
    }
  }
}

//** III.串口通讯 部分-------------------------------------------
//  1.基础功能
//拒绝请求
void Refuse() {
  Serial.println("404");
}
//接受请求
void Accept() {
  Serial.println("200");
}
//处理请求

/*
  void Request(String ReqID) {
  if (washingStatus == 0) {
    if (ReqID == "1") {
      Accept();
      DoFunction(111);
    } else if (ReqID == "2") {
      Accept();
      DoFunction(112);
    } else if (ReqID == "3") {
      Accept();
      DoFunction(113);
    } else if (ReqID == "4") {
      Accept();
      DoFunction(121);
    } else if (ReqID == "5") {
      Accept();
      DoFunction(122);
    } else if (ReqID == "6") {
      Accept();
      DoFunction(123);
    } else {
      Refuse();
    }
  }
  else if (washingStatus != 0) {  //暂停
    if (ReqID == "7") {
      Accept();
      WorkingFunction(1);
    } else if (ReqID == "8") {  //终止
      Accept();
      WorkingFunction(2);
    } else if (ReqID == "9") {  //继续执行
      if (ListPageIndex == 2) {
        Accept();
        WorkingFunction(3);
      }
      else {
        Refuse();
      }
    } else if (ReqID == "0") {  //完成
      if (washingStatus == 9) {
        Accept();
        WorkingFunction(9);
      }
      else {
        Refuse();
      }
    } else {
      Refuse();
    }
  }
  }
*/
void Request(int ReqID) {
  if (washingStatus == 0) {
    switch (ReqID)
    {
      case 111:
      case 112:
      case 113:
      case 121:
      case 122:
      case 123:
        Accept();
        DoFunction(ReqID);
        break;
      default:
        Refuse();
        break;
    }
  }
  else if (washingStatus != 0) {  //暂停
    switch (ReqID)
    {
      case 1:
      case 2:
        Accept();
        WorkingFunction(ReqID);
        break;
      case 3:
        if (ListPageIndex == 2) {
          Accept();
          WorkingFunction(3);
        }
        else {
          Refuse();
        }
        break;
      case 9:
        if (washingStatus == 9) {
          Accept();
          WorkingFunction(9);
        }
        else {
          Refuse();
        }
        break;
      default:
        Refuse();
        break;
    }
  }
}
/*============================================================================================================================================================*/
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
//  线程进行内容
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/*============================================================================================================================================================*/
void Action::setup() {   //事务处理设定
  /*输入信号*/
  pinMode(WaterIn, INPUT);
  pinMode(WaterOut, INPUT);
  pinMode(OrderButtonSignal1, INPUT);
  pinMode(OrderButtonSignal2, INPUT);
  pinMode(OrderButtonSignal3, INPUT);
  /*输出信号*/
  pinMode(MotorReg, OUTPUT);
  pinMode(MotorRes, OUTPUT);
  pinMode(Invalve, OUTPUT);
  pinMode(Outvalve, OUTPUT);
}

void Listener::setup() {  //监听设定
  u8g2.begin();
  u8g2.setPowerSave(0);
  SetupOrderList(ListPageIndex);
  setOrder(OrderIndex);
}

void ComSerial::setup() {
  Serial.begin(9600);
  Serial.println(washingStatus);
}

void Action::loop() {   //事务处理循环
  if (washingStatus == 0) {
    switch (actionTrigger) {
      case 0: break;
      case 1:
        wash(1);
        break;
      case 2:
        wash(2);
        break;
      case 3:
        wash(3);
        break;
      case 4:
        dry(1);
        break;
      case 5:
        dry(2);
        break;
      case 6:
        dry(3);
        break;
      default: break;
    }
  }
}

void Listener::loop() {  //监听循环
  Decode();
  if (washingStatus != tempStatus) {
    tempStatus = washingStatus;
    Serial.println(washingStatus);
  }
  if (washingStatus == 9) {
    WorkingFunction(9);
  }

}

void ComSerial::loop() {
  if (Serial.available() > 0) {
    while (Serial.available() > 0) {
      SerialRead += char(Serial.read());
      sleep(4);
    }
    if (SerialRead.length() > 0) {
      Serial.println("nih");
      Request(SerialRead.toInt());
    }
  }
}

void setup() {
  // put your setup code here, to run once:
  mySCoop.start();
}

void loop() {
  // put your main code here, to run repeatedly:
  yield();
}
