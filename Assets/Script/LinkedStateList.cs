using System.Collections;

namespace LinkedStateList
{
    public class LinkedStateList<T> : IEnumerable
    {
        public LinkedStateListNode<T> CurrentState { private set; get; }

        public LinkedStateListNode<T> First { private set; get; }
        public LinkedStateListNode<T> Last { private set; get; }



        // Checks if a next/previous state exists
        public bool CanGoToNext { get { return CurrentState.Next != null; } }
        public bool CanGoToPrev { get { return CurrentState.Prev != null; } }



        // Size
        public int Count { get { return Last.index + 1; } }
        public int StateIndex { get { return CurrentState.index; } }



        // Resets the list to a single-element list containing only the given element
        public LinkedStateListNode<T> SetFirst (T value)
        {
            CurrentState = First = Last = new LinkedStateListNode<T> (value);
            First.index = 0;

            return First;
        }

        // Adds a new element after the current state node and sets the current state to it
        // Any nodes after the current state node are discarded
        // Returns the new node
        public LinkedStateListNode<T> Add (T value)
        {
            if (CurrentState == null)
                return SetFirst (value);
            
            CurrentState.Next = new LinkedStateListNode<T> (value);
            CurrentState = CurrentState.Next;
            Last = CurrentState;
            
            CurrentState.index = CurrentState.Prev.index + 1;

            return CurrentState;
        }

        // Adds a new element after the current state node but does not change the current state
        // Any nodes after the current state node are discarded
        // Inconsistent if called while the list is empty
        // Returns the new node
        public LinkedStateListNode<T> AddWithoutStateChange (T value)
        {
            if (CurrentState == null)
                return SetFirst (value);
        
            CurrentState.Next = new LinkedStateListNode<T> (value);
            Last = CurrentState.Next;
            
            CurrentState.Next.index = CurrentState.index + 1;

            return CurrentState.Next;
        }

        // Set current state forwards a node
        public T EnterNextState ()
        {
            if (!CanGoToNext)
                throw new InvalidNodeException ("There is no next state to enter.");

            T ret = CurrentState.Next.Value;

            CurrentState = CurrentState.Next;
            return ret;
        }

        // Sets current state back a node
        public T EnterPrevState ()
        {
            if (!CanGoToPrev)
                throw new InvalidNodeException ("There is no previous state to enter.");

            T ret = CurrentState.Prev.Value;

            CurrentState = CurrentState.Prev;
            return ret;
        }



        // To string
        public override string ToString ()
        {
            if (First == null)
                return "<>";

            string output = "<" + (First == CurrentState ? "+" : First.index.ToString ());

            for (LinkedStateListNode<T> node = First.Next; node != null; node = node.Next)
            {
                output += ", " + (node == CurrentState ? "+" : node.index.ToString ());
            }

            return output + ">";
        }



        // To array
        public T[] ToArray ()
        {
            T[] array = new T[Count];

            int i = 0;
            foreach (T t in this)
            {
                array[i] = t;
                i++;
            }

            return array;
        }

        // From array
        public static LinkedStateList<T> FromArray (T[] array, int stateIndex)
        {
            LinkedStateList<T> list = new LinkedStateList<T> ();

            int i = 0;
            foreach (T t in array)
            {
                if (i <= stateIndex)
                    list.Add (t);
                else
                    list.AddWithoutStateChange (t);

                i++;
            }

            return list;
        }



        // Indexers
        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfBoundsException ("Index exceeds size of list.");

                LinkedStateListNode<T> node = First;
                int i = 0;
                while (node != null)
                {
                    if (i == index)
                        return node.Value;

                    node = node.Next;
                    i++;
                }

                throw new IndexOutOfBoundsException ("Index exceeds size of list.");
            }
        }

        // Iterator
        public IEnumerator GetEnumerator ()
        {
            LinkedStateListNode<T> node = First;

            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }
    }



    // Node class
    public class LinkedStateListNode<T>
    {
        private LinkedStateListNode<T> next, prev;
        public LinkedStateListNode<T> Next { internal set { next = value; value.prev = this; } get { return next; } }
        public LinkedStateListNode<T> Prev { internal set { prev = value; value.next = this; } get { return prev; } }
        public T Value { get; set; }

        internal int index;

        public LinkedStateListNode (T value)
        {
            Value = value;
        }
    }



    // Exceptions
    public class InvalidNodeException : System.Exception
    {
        public InvalidNodeException (string message) : base (message) { }
        public InvalidNodeException (string message, System.Exception innerException) : base (message, innerException) { }
    }
    public class IndexOutOfBoundsException : System.Exception
    {
        public IndexOutOfBoundsException (string message) : base (message) { }
        public IndexOutOfBoundsException (string message, System.Exception innerException) : base (message, innerException) { }
    }
}