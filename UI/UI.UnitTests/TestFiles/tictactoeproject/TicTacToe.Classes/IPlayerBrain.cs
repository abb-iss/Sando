using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public interface IPlayerBrain
    {
        GameMove Think(XOSymbol[,] gamePad, XOSymbol xoSymbol);

        List<GamePadNode> GetGamePadNodes();
    }
}
