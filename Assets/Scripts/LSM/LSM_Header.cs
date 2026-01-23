using UnityEngine;
namespace LSM
{
    public interface I_Observer
    {
        public void AddSubject(int mod);
        public void RemoveSubject(int mod);
        public void Notify();
    }

    public interface I_Subject
    {
        public void Subscribe(int mod, I_Observer _observer);
        public void UnSubscribe(int mod, I_Observer _observer);
        public void Notify();
    }
}