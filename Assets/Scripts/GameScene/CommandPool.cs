using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandPool : MonoBehaviour
{
    public static CommandPool Inst;

    [SerializeField]
    private List<CommandPrefab> commandPrefabs;

    private List<Command> activeCommands = new List<Command>();
    private List<Command> innactiveCommands = new List<Command>();

    void Awake()
    {
        Inst = this;

        foreach(CommandPrefab commandPrefab in commandPrefabs)
        {
            for(int i = 0; i < commandPrefab.StartingPool; i++)
            {
                innactiveCommands.Add(Instantiate<Command>(commandPrefab.Prefab, Vector3.zero, Quaternion.identity, transform));
            }
        }
    }

    public Command GetRandomCommand()
    {
        int sumBias = 0;
        foreach(CommandPrefab commandPrefab in commandPrefabs)
        {
            sumBias += commandPrefab.FrequencyBias;
        }
        int random = UnityEngine.Random.Range(0, sumBias);
        foreach (CommandPrefab commandPrefab in commandPrefabs)
        {
            if (random < commandPrefab.FrequencyBias)
            {
                return GetCommand(commandPrefab);
            }
            random -= commandPrefab.FrequencyBias;
        }
        Debug.Assert(false, "Random seems to be Greater Than or Equal To sumBias.  This shouldn't be possible.");
        return null;
    }

    public T GetCommand<T>() where T : Command
    {
        foreach(Command command in innactiveCommands)
        {
            if(command.GetType() == typeof(T))
            {
                unpool(command);
                return (T)command;
            }
        }
        T newCommand = InstantiateCommand<T>();
        activeCommands.Add(newCommand);
        return newCommand;
    }

    public Command GetCommand(CommandPrefab commandPrefab)
    {
        foreach(Command command in innactiveCommands)
        {
            if(command.GetType() == commandPrefab.Prefab.GetType())
            {
                unpool(command);
                return command;
            }
        }
        Command newCommand = InstantiateCommand(commandPrefab);
        activeCommands.Add(newCommand);
        return newCommand;
    }

    public Command GetCommandAtSlot(CommandSlot slot)
    {
        foreach (Command command in activeCommands)
        {
            if (command.currentSlot == slot)
                return command;
        }
        Debug.Assert(false, "No Command Found at Slot: " + slot.name);
        return null;
    }

    public void ReturnToPool(Command command)
    {
        command.Deactivate();
        pool(command);
    }

    private T InstantiateCommand<T>() where T : Command
    {
        foreach(CommandPrefab commandPrefab in commandPrefabs)
        {
            if (commandPrefab.Prefab.GetType() == typeof(T))
                return (T)Instantiate<Command>(commandPrefab.Prefab, Vector3.zero, Quaternion.identity, transform);
        }
        Debug.Assert(false, "No CommandPrefab found with type: " + typeof(T).ToString());
        return null;
    }

    private Command InstantiateCommand(CommandPrefab commandPrefab)
    {
        return Instantiate<Command>(commandPrefab.Prefab, Vector3.zero, Quaternion.identity, transform);
    }

    private void pool(Command command)
    {
        activeCommands.Remove(command);
        innactiveCommands.Add(command);
    }

    private void unpool(Command command)
    {
        innactiveCommands.Remove(command);
        activeCommands.Add(command);
    }
}

[Serializable]
public class CommandPrefab
{
    public Command Prefab;
    public int FrequencyBias;
    public int StartingPool;
}
