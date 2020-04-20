using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandRegister : MonoBehaviour
{
    public static CommandRegister Inst;

    [SerializeField]
    private List<CommandSlot> commandSlots;

    void Awake()
	{
        Inst = this;
	}

    void Start()
    {
        bool lastCommandWasTurn = false;
        foreach (CommandSlot commandSlot in commandSlots)
        {
            if (lastCommandWasTurn)
            {
                Command newCommand = CommandPool.Inst.GetCommand<GoStraightCommand>();
                newCommand.Activate(commandSlot);
                lastCommandWasTurn = false;
            }
            else
            {
                Command newCommand = CommandPool.Inst.GetRandomCommand();
                newCommand.Activate(commandSlot);
                lastCommandWasTurn = (newCommand.GetType() != typeof(GoStraightCommand));
            }
        }
    }

    public Command GetCommand(int slotNumber = 0)
	{
        Debug.Assert(commandSlots != null, "Looks like you forgot to plug in the command slots");
        Debug.Assert(slotNumber < commandSlots.Count, "Tyring to get command at slot number: " + slotNumber + " , but there are only " + commandSlots.Count + " slots.");
        return CommandPool.Inst.GetCommandAtSlot(commandSlots[slotNumber]);
	}
}
