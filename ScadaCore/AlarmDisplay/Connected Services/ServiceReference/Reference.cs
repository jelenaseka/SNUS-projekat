﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlarmDisplay.ServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IAlarmDisplayService", CallbackContract=typeof(AlarmDisplay.ServiceReference.IAlarmDisplayServiceCallback))]
    public interface IAlarmDisplayService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAlarmDisplayService/initialize", ReplyAction="http://tempuri.org/IAlarmDisplayService/initializeResponse")]
        void initialize();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAlarmDisplayService/initialize", ReplyAction="http://tempuri.org/IAlarmDisplayService/initializeResponse")]
        System.Threading.Tasks.Task initializeAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAlarmDisplayServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAlarmDisplayService/OnAlarmInvoked")]
        void OnAlarmInvoked(string alarm);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAlarmDisplayServiceChannel : AlarmDisplay.ServiceReference.IAlarmDisplayService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AlarmDisplayServiceClient : System.ServiceModel.DuplexClientBase<AlarmDisplay.ServiceReference.IAlarmDisplayService>, AlarmDisplay.ServiceReference.IAlarmDisplayService {
        
        public AlarmDisplayServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public AlarmDisplayServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public AlarmDisplayServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public AlarmDisplayServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public AlarmDisplayServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public void initialize() {
            base.Channel.initialize();
        }
        
        public System.Threading.Tasks.Task initializeAsync() {
            return base.Channel.initializeAsync();
        }
    }
}
