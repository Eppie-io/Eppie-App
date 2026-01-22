// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using NUnit.Framework;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public class BaseAddressSettingsModelTests
    {
        private class TestAddressSettingsModel : BaseAddressSettingsModel
        {
            public TestAddressSettingsModel() : base()
            {
            }

            public TestAddressSettingsModel(Account account) : base(account)
            {
            }
        }

        [Test]
        public void ExternalContentPolicyValuesReturnsAllEnumValues()
        {
            // Arrange
            var model = new TestAddressSettingsModel();

            // Act
            var values = model.ExternalContentPolicyValues;

            // Assert
            Assert.That(values, Is.Not.Null);
            Assert.That(values.Length, Is.EqualTo(3));
            Assert.That(values, Contains.Item(ExternalContentPolicy.AlwaysAllow));
            Assert.That(values, Contains.Item(ExternalContentPolicy.AskEachTime));
            Assert.That(values, Contains.Item(ExternalContentPolicy.Block));
        }

        [Test]
        public void InstanceExternalContentPolicyValuesReturnsSameAsStatic()
        {
            // Arrange
            var model = new TestAddressSettingsModel();

            // Act
            var staticValues = model.ExternalContentPolicyValues;
            var instanceValues = model.ExternalContentPolicyValues;

            // Assert
            Assert.That(instanceValues, Is.SameAs(staticValues));
        }

        [Test]
        public void ExternalContentPolicyDefaultsToAlwaysAllow()
        {
            // Arrange & Act
            var model = new TestAddressSettingsModel();

            // Assert
            Assert.That(model.ExternalContentPolicy, Is.EqualTo(ExternalContentPolicy.AlwaysAllow));
        }

        [Test]
        public void ExternalContentPolicyInitializesFromAccount()
        {
            // Arrange
            var account = new Account
            {
                Email = new EmailAddress("test@example.com", "Test User"),
                ExternalContentPolicy = ExternalContentPolicy.Block
            };

            // Act
            var model = new TestAddressSettingsModel(account);

            // Assert
            Assert.That(model.ExternalContentPolicy, Is.EqualTo(ExternalContentPolicy.Block));
        }

        [Test]
        public void ExternalContentPolicyCanBeChanged()
        {
            // Arrange
            var model = new TestAddressSettingsModel();
            model.ExternalContentPolicy = ExternalContentPolicy.AlwaysAllow;

            // Act
            model.ExternalContentPolicy = ExternalContentPolicy.AskEachTime;

            // Assert
            Assert.That(model.ExternalContentPolicy, Is.EqualTo(ExternalContentPolicy.AskEachTime));
        }

        [Test]
        public void ToAccountIncludesExternalContentPolicy()
        {
            // Arrange
            var originalAccount = new Account
            {
                Email = new EmailAddress("test@example.com", "Test User"),
                ExternalContentPolicy = ExternalContentPolicy.AlwaysAllow
            };
            var model = new TestAddressSettingsModel(originalAccount);
            model.ExternalContentPolicy = ExternalContentPolicy.Block;

            // Act
            var resultAccount = model.ToAccount();

            // Assert
            Assert.That(resultAccount, Is.Not.Null);
            Assert.That(resultAccount.ExternalContentPolicy, Is.EqualTo(ExternalContentPolicy.Block));
        }

        [Test]
        public void ExternalContentPolicyRaisesPropertyChanged()
        {
            // Arrange
            var model = new TestAddressSettingsModel();
            bool propertyChangedRaised = false;
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(BaseAddressSettingsModel.ExternalContentPolicy))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            model.ExternalContentPolicy = ExternalContentPolicy.Block;

            // Assert
            Assert.That(propertyChangedRaised, Is.True);
        }

        [Test]
        public void ExternalContentPolicyDoesNotRaisePropertyChangedWhenSameValue()
        {
            // Arrange
            var model = new TestAddressSettingsModel();
            model.ExternalContentPolicy = ExternalContentPolicy.AlwaysAllow;
            bool propertyChangedRaised = false;
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(BaseAddressSettingsModel.ExternalContentPolicy))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act - set to the same value
            model.ExternalContentPolicy = ExternalContentPolicy.AlwaysAllow;

            // Assert
            Assert.That(propertyChangedRaised, Is.False);
        }
    }
}
