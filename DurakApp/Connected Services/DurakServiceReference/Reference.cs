﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DurakApp.DurakServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DurakServiceReference.IDurakService")]
    public interface IDurakService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/HasPassword", ReplyAction="http://tempuri.org/IDurakService/HasPasswordResponse")]
        bool HasPassword(string RoomName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/HasPassword", ReplyAction="http://tempuri.org/IDurakService/HasPasswordResponse")]
        System.Threading.Tasks.Task<bool> HasPasswordAsync(string RoomName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/IsFree", ReplyAction="http://tempuri.org/IDurakService/IsFreeResponse")]
        bool IsFree(string RoomName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/IsFree", ReplyAction="http://tempuri.org/IDurakService/IsFreeResponse")]
        System.Threading.Tasks.Task<bool> IsFreeAsync(string RoomName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/CreateRoom", ReplyAction="http://tempuri.org/IDurakService/CreateRoomResponse")]
        bool CreateRoom(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/CreateRoom", ReplyAction="http://tempuri.org/IDurakService/CreateRoomResponse")]
        System.Threading.Tasks.Task<bool> CreateRoomAsync(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/DeleteRoom", ReplyAction="http://tempuri.org/IDurakService/DeleteRoomResponse")]
        bool DeleteRoom(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/DeleteRoom", ReplyAction="http://tempuri.org/IDurakService/DeleteRoomResponse")]
        System.Threading.Tasks.Task<bool> DeleteRoomAsync(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/ConnectRoom", ReplyAction="http://tempuri.org/IDurakService/ConnectRoomResponse")]
        bool ConnectRoom(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/ConnectRoom", ReplyAction="http://tempuri.org/IDurakService/ConnectRoomResponse")]
        System.Threading.Tasks.Task<bool> ConnectRoomAsync(string RoomName, string password, int UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/GetFreeRooms", ReplyAction="http://tempuri.org/IDurakService/GetFreeRoomsResponse")]
        string[] GetFreeRooms();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDurakService/GetFreeRooms", ReplyAction="http://tempuri.org/IDurakService/GetFreeRoomsResponse")]
        System.Threading.Tasks.Task<string[]> GetFreeRoomsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDurakServiceChannel : DurakApp.DurakServiceReference.IDurakService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DurakServiceClient : System.ServiceModel.ClientBase<DurakApp.DurakServiceReference.IDurakService>, DurakApp.DurakServiceReference.IDurakService {
        
        public DurakServiceClient() {
        }
        
        public DurakServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DurakServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DurakServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DurakServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool HasPassword(string RoomName) {
            return base.Channel.HasPassword(RoomName);
        }
        
        public System.Threading.Tasks.Task<bool> HasPasswordAsync(string RoomName) {
            return base.Channel.HasPasswordAsync(RoomName);
        }
        
        public bool IsFree(string RoomName) {
            return base.Channel.IsFree(RoomName);
        }
        
        public System.Threading.Tasks.Task<bool> IsFreeAsync(string RoomName) {
            return base.Channel.IsFreeAsync(RoomName);
        }
        
        public bool CreateRoom(string RoomName, string password, int UserID) {
            return base.Channel.CreateRoom(RoomName, password, UserID);
        }
        
        public System.Threading.Tasks.Task<bool> CreateRoomAsync(string RoomName, string password, int UserID) {
            return base.Channel.CreateRoomAsync(RoomName, password, UserID);
        }
        
        public bool DeleteRoom(string RoomName, string password, int UserID) {
            return base.Channel.DeleteRoom(RoomName, password, UserID);
        }
        
        public System.Threading.Tasks.Task<bool> DeleteRoomAsync(string RoomName, string password, int UserID) {
            return base.Channel.DeleteRoomAsync(RoomName, password, UserID);
        }
        
        public bool ConnectRoom(string RoomName, string password, int UserID) {
            return base.Channel.ConnectRoom(RoomName, password, UserID);
        }
        
        public System.Threading.Tasks.Task<bool> ConnectRoomAsync(string RoomName, string password, int UserID) {
            return base.Channel.ConnectRoomAsync(RoomName, password, UserID);
        }
        
        public string[] GetFreeRooms() {
            return base.Channel.GetFreeRooms();
        }
        
        public System.Threading.Tasks.Task<string[]> GetFreeRoomsAsync() {
            return base.Channel.GetFreeRoomsAsync();
        }
    }
}
