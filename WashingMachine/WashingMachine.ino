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
    analogWrite(MotorReg, 50);
    //digitalWrite(MotorReg, HIGH);
    sleep(2000);
    digitalWrite(MotorReg, LOW);
    sleep(1000);
    analogWrite(MotorRes, 50);
    //digitalWrite(MotorRes, HIGH);
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

/*  模块名称：绘制显示内容
 *  模块说明：该模块中可以完成对
 * 
*/
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

/*   模块名称:菜单显示内容预设
     模块说明：
        该模块中规定了每个菜单页面的页码，以及每个菜单页面的显示内容，关系如下图
*/
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

/*  模块名称：菜单任务执行
    模块说明:
       该模块通过传递回的参数，执行相应洗衣操作
*/
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

/*重置函数*/
void(* resetFunc) (void) = 0;

/*  模块名称：执行进行中任务
    模块说明：
       当洗衣机处于运行状态时，可对洗衣机发出：暂停、继续、终止和返回主页菜单4种指令
    1.暂停指令：
       在洗衣过程中，偶尔会需要中途暂停当前洗衣却不影响整个洗衣流程时，可以使用暂停功能。SCoop库中提供了子线程的暂停函数，可以在监视线程中通过调用pause()函数实现对作业线程进行暂停操作，保存当前菜单页面，跳转进入暂停页面，同时关闭电机，进而达到暂停当前洗衣作业的效果。
    2.停止指令：
       当洗衣中途需要停止当前洗衣作业时，可使用停止指令终止当前的工作。此设计中所使用的SCoop库并未提供线程的重置方法，通常在停止洗衣时，都是终止当前操作，返回到最开始状态，故此，重置Arduino本身也可以实现。Arduino中内置了一个名为resetFunc()的函数，可以通过声明该函数地址为0，当调用该函数时可以达到重置Arduino的作用。
    3.继续指令：
       当暂停洗衣后，继续执行之前暂停的洗衣任务，可以通过继续指令实现。调用SCoop库中提供的resume()函数可以实现使子线程继续执行，实现继续作业的效果。
    9.当洗衣完成后，跳转至完成页面，等待继续操作。
*/
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
      resetFunc();
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
/* 模块名称：矩阵键盘编码按钮信号
   模块说明：
      在该模块中，对读取到的三个按钮信号引脚的电平信号进行编码，规定：低电平为1，高电平为0，通过5个按钮输入的信号，按照二进制编码，可有(000)、(001)、(010)、(011)、(100)、(101)一共6种组合信号，再将其转换为十进制，可以得到0-5。
      Arduino使用循环loop方法中的程序段实现连续性的运作，由于Arduino的loop执行频率很高，当按钮按下时不能做到只通过单个有效的电平信号，同时，在一次按钮按下时，可能会出现多次接触，进而产生多个信号，干扰正常操作流程，故此，需要做抖动处理：
      首先降低定义当前的信号位状态Sig,上一个信号位状态位SigBefore，当第一个信号输入时，将其赋值给SigBefore，之后如若Sig与SigBefore不相等时，则判断Sig为有效输入信号，将Sig作为函数参数，传递给按钮功能执行函数，实现其对应功能，再将Sig的值赋值给SigBefore；如若Sig与SigBefore相等时,不做任何处理，将Sig的值赋值给SigBefore。
*/
//矩阵键盘编码
void Decode() {
  int Sig1 = digitalRead(OrderButtonSignal1);
  int Sig2 = digitalRead(OrderButtonSignal2);
  int Sig3 = digitalRead(OrderButtonSignal3);
  int Sig = (Sig1 == HIGH ? 0 : 1) * 4 + (Sig2 == HIGH ? 0 : 1) * 2 + (Sig3 == HIGH ? 0 : 1) * 1; /*信号编码转为十进制*/
  /*矩阵键盘编码防抖动*/
  if (SigBefore != Sig) {
    OrderButtonFunction(Sig);
  }
  sleep(50);
  SigBefore = Sig;
}

//矩阵编码控制
/* 模块名称：矩阵编码控制
   模块说明：
      该函数取用编码后的按钮信号，取用0-5一共6种信号状态，按钮和对应的信号值以及对应功能匹配关系如表：//表格
      首先需要判断当前菜单是否可执行操作：当洗衣状态为：待机中，即washingStatus为0时，可执行以下操作：
      1.选项光标下移操作：
          通过菜单页表中可以获取到当前显示页面菜单项目栏数ListCount，如果光标位置OrderIndex小于菜单项目栏数ListCount，即光标还未到菜单底部，可以下移，将OrderIndex值加1，若OrderIndex等于菜单项目栏数ListCount，则回到第一项，即将OrderIndex的值设为1，将OrderIndex作为函数参数传递给菜单刷新函数setOrder。
      2.选项光标上移操作：
          通过菜单页表中可以获取到当前显示页面菜单项目栏数ListCount，如果光标位置OrderIndex大于1，即光标还未到菜单顶部，可以上移，将OrderIndex值加1，若OrderIndex等于1，则跳转到最后一项，即将OrderIndex的值设为菜单项目栏数ListCount，将OrderIndex作为函数参数传递给菜单刷新函数setOrder。
      3.确定选择当前选项：
          参考《菜单表逻辑》，判断当前菜单是否为主菜单，若为主菜单，则：
              目标菜单页码 = 当前菜单页码 * 10 + 当前选项光标位置
          将当前光标位置复位到1，跳转至目标菜单；
          若当前菜单为子菜单，则：
              目标功能编号 = 当前菜单页码 * 10 + 当前选项光标位置
          由于同级子菜单功能执行页面显示窗口一致，故：
              目标功能显示 = 当前菜单页码 * 10
          将当前光标位置复位到1，跳转至目标窗口
      4.返回上级菜单：
          判断当前菜单页面是否为主菜单，若为主菜单，不做任何处理，反之
              目标菜单页码 = 取整[当前菜单页码 / 10]
          将当前光标位置复位到1，跳转至目标菜单
      5.返回主页
          跳转至主菜单，光标位置复位到1
      若洗衣状态为：已完成洗衣，则按下任意按钮回到主菜单
      若为其他洗衣状态：
      1.选项光标下移
      2.选项光标上移
      3.执行选中指令
      5.取消当前正在执行任务

*/
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
        if (ListPageIndex / 10 == 1) {//子菜单
          DoFunction(ListPageIndex * 10 + OrderIndex);
          ListPageIndex = ListPageIndex * 10;
          OrderIndex = 1;
          SetupOrderList(ListPageIndex);
          setOrder(OrderIndex);
        }
        else {//主菜单
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
    washingStatus = 0;
    actionTrigger = 0;
    ListPageIndex = 1;
    OrderIndex = 1;
    SetupOrderList(ListPageIndex);
    setOrder(OrderIndex);
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
      case 5:
        WorkingFunction(2);
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
void Request(int ReqID) {
  if (ReqID == 100) {//在线状态反馈
    Serial.println("Y");
  }
  else if (washingStatus == 0) {
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
        ListPageIndex = (ReqID / 10) * 10;
        OrderIndex = 1;
        SetupOrderList(ListPageIndex);
        setOrder(OrderIndex);
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
          OrderButtonFunction(3);//模拟按钮输入确认指令
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
  Serial.println("0");
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

}

void setup() {
  Serial.begin(9600);

  // put your setup code here, to run once:
  mySCoop.start();
}

void loop() {
  if (Serial.available() > 0) {
    String readout = "";
    while (Serial.available() > 0) {
      readout += char(Serial.read());
      delay(2);
    }
    if (readout.length() > 0) {
      Request(readout.toInt());
    }
  }
  // put your main code here, to run repeatedly:
  yield();
}
