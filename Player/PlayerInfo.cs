using System.Collections;
using System;

public class PlayerInfo:IComparable<PlayerInfo>{	//PlayerInfo实现IComparable接口
	public string playerName;	//玩家姓名
	public int playerScore;		//玩家得分
    public int playerDeath;
		
	//无参构造函数
	public PlayerInfo(){
	}

	//构造函数：玩家姓名，玩家得分
	public PlayerInfo(string _playerName,int _playerScore,int _playerDeath){
		playerName = _playerName;
		playerScore = _playerScore;
        playerDeath = _playerDeath;
	}

	//实现IComparable接口的CompareTo函数，完成PlayerInfo对象的比较
	public int CompareTo(PlayerInfo other){
        if (this.playerScore > other.playerScore)
            return 1;
        else if (this.playerScore == other.playerScore)
        {
            if (this.playerDeath < other.playerDeath)
                return 1;
            else if (this.playerDeath == other.playerDeath)
                return 0;
            else return -1;
        }
        else
            return -1;
	}
}
