// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using EasyNetQ.AdvancedMessagePolymorphism;
using EasyNetQ.Topology;
using NUnit.Framework;
using Rhino.Mocks;
using System.Threading.Tasks;
using System.Linq;

namespace EasyNetQ.Tests.AdvancedMessagePolymorphismTest
{
    [TestFixture]
    public class AdvancedPolymorphismPublishExchangeDeclareStrategyTests
    {
        [Test]
        public void When_declaring_exchanges_for_message_type_that_has_no_interface_one_exchange_created()
        {
            var advancedBus = MockRepository.GenerateStub<IAdvancedBus>();

            advancedBus.Stub(b => b.ExchangeDeclareAsync(
                Arg<string>.Is.Anything, 
                Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything))
                .Return(Task.FromResult<IExchange>(MockRepository.GenerateStub<IExchange>()));

            advancedBus.Stub(b => b.BindAsync(Arg<IExchange>.Is.Anything, Arg<IExchange>.Is.Anything, Arg<string>.Is.Anything))
                .Return(Task.FromResult<IBinding>(MockRepository.GenerateStub<IBinding>()));

            advancedBus.Stub(c => c.Container.Resolve<IConventions>())
                .Return(new Conventions(new TypeNameSerializer()));

            var publishExchangeStrategy = new AdvancedPolymorphismPublishExchangeDeclareStrategy();


            publishExchangeStrategy.DeclareExchangeAsync(advancedBus, typeof(MyMessage), ExchangeType.Topic).Wait();

            //ensure that only one exchange is declared
            advancedBus.AssertWasCalled(m => m.ExchangeDeclareAsync(
                Arg<string>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything), opt => opt.Repeat.Times(1));

            //ensure that the exchange is not bound to anything
            advancedBus.AssertWasNotCalled(b => b.BindAsync(Arg<IExchange>.Is.Anything, Arg<IExchange>.Is.Anything, Arg<string>.Is.Anything));
        }

        //[Test]
        //public void When_declaring_exchanges_for_versioned_message_exchange_per_version_created_and_bound_to_superceding_version()
        //{
        //    var exchanges = new List<ExchangeStub>();
        //    var bus = CreateAdvancedBusMock(exchanges.Add, BindExchanges, t => t.Name);
        //    var publishExchangeStrategy = new AdvancedPolymorphismPublishExchangeDeclareStrategy();

        //    publishExchangeStrategy.DeclareExchange(bus, typeof(MessageWithMultipleInterfaces), ExchangeType.Topic);

        //    Assert.That(exchanges, Has.Count.EqualTo(3), "Two exchanges should have been created");
        //    Assert.That(exchanges[0].Name, Is.EqualTo("MessageWithMultipleInterfaces"), "Concrete message exchange should been created first");

        //    Assert.That(exchanges.Count(m => m.Name == "IMessageInterfaceOne"), Is.EqualTo(1), "One IMessageInterfaceOne exchange should be created");
        //    Assert.That(exchanges.Count(m => m.Name == "IMessageInterfaceTwo"), Is.EqualTo(1), "One IMessageInterfaceTwo exchange should be created");
        //    Assert.That(exchanges[0].BoundTo, Is.Null, "Superseded message exchange should route messages anywhere");
        //    Assert.That(exchanges[1].BoundTo, Is.EqualTo(exchanges[0]), "Superseding message exchange should route message to superseded exchange");
        //    Assert.That(exchanges[2].BoundTo, Is.EqualTo(exchanges[0]), "Superseding message exchange should route message to superseded exchange");
        //}
    }
}