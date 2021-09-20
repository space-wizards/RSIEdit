using System.Collections.Generic;
using Importer.RSI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Models.RSI
{
    public class RsiItem
    {
        public RsiItem(Rsi? rsi = null)
        {
            Rsi = rsi ?? new Rsi();
            foreach (var rsiState in Rsi.States)
            {
                _rsiUpdateStates[rsiState.Name] = UpdateState.None;
            }
        }

        public Rsi Rsi { get; }

        private Dictionary<string, UpdateState> _rsiUpdateStates = new();

        public RsiSize Size => Rsi.Size;

        public void MarkAsUpdated(RsiState state, UpdateState updateState)
        {
            _rsiUpdateStates[state.Name] = updateState;
        }
        
        public UpdateState WasModified(RsiState state)
        {
            if (_rsiUpdateStates.TryGetValue(state.Name, out var wasModified)) 
                return wasModified;
            
            // Assume the state is new and track it
            _rsiUpdateStates.Add(state.Name, UpdateState.Edit);
            return UpdateState.Edit;
        }
        
        public void LoadImage(int index, Image<Rgba32> image)
        {
            Rsi.States[index].LoadImage(image, Size);
        }

        public void AddState(RsiImage image)
        {
            Rsi.States.Add(image.State);
            _rsiUpdateStates.Add(image.State.Name, UpdateState.Edit);
        }

        public void InsertState(int index, RsiImage image)
        {
            Rsi.States.Insert(index, image.State);
            _rsiUpdateStates.TryAdd(image.State.Name, UpdateState.Edit);
        }

        public void RemoveState(int index)
        {
            Rsi.States.RemoveAt(index);
        }

        public void RemoveState(RsiState state)
        {
            var index = Rsi.States.IndexOf(state);

            if (index != -1)
            {
                RemoveState(index);
            }
        }
    }
    
    public enum UpdateState : byte
    {
        None,
        Edit
    }
}
