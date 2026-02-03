using DynamicData;
using DynamicData.Binding;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace usbprison
{
    public class CustomAdaptor : IObservableCollectionAdaptor<MultiTrackedDeviceViewModel, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionAdaptor{TObject, TKey}"/> class.
        /// </summary>
        /// <param name="options"> The binding options.</param>
        public CustomAdaptor()
        {
        }

        /// <summary>
        /// Maintains the specified collection from the changes.
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <param name="collection">The collection.</param>
        public void Adapt(IChangeSet<MultiTrackedDeviceViewModel, string> changes, IObservableCollection<MultiTrackedDeviceViewModel> collection)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            {
                //using (collection.SuspendCount())
                //{
                    DoUpdate(changes, collection);
                //}
            }
        }

        private void DoUpdate(IChangeSet<MultiTrackedDeviceViewModel, string> changes, IObservableCollection<MultiTrackedDeviceViewModel> list)
        {
            foreach (var change in changes)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                        list.Add(change.Current);
                        break;

                    case ChangeReason.Remove:
                        var target = list.FirstOrDefault(x => x.Id == change.Current.Id);
                        if (target != null)
                            list.RemoveAt(list.IndexOf(target));
                        //list.RemoveAt(change.PreviousIndex);
                        break;

                    case ChangeReason.Update:

                        var previous = change.Previous.Value;
                        var current = change.Current;

                        if (!previous.InPrison && current.InPrison)
                        {
                            list.Remove(previous);
                            list.Add(current);
                        }
                        else if (previous.InPrison && !current.InPrison && previous.MachineId == current.MachineId)
                        {
                            previous.Update();  // update so it doesn't auto renew
                        }
                        else if (previous.InPrison && !current.InPrison && previous.MachineId != current.MachineId)
                        {
                            // do nothing until inprison status disappears
                        }
                        else if (!previous.InPrison && !current.InPrison)
                        {
                            previous.AddMachines(current.Machines);
                        }

                        break;
                }
            }
        }
    }
}
