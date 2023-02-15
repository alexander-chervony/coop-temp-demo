Vue.component('immovables-item',
    {
        props: ['immovables','index', 'type', 'accumulated','isfounder'],
        template: '<tr>\
            <th scope="row">{{index + 1}}</th>\
            <td><b-button variant="link" v-on:click="$emit(\'impersonate\', immovables)">{{immovables.FirstName}} {{immovables.MiddleName}} {{immovables.SurnameInitial}}</b-button></td>\
            <td v-b-tooltip.hover :title="\'Стоимость недвижимости\'">{{immovables.PriceFormatted}}</td>\
            <td v-b-tooltip.hover :title="\'Накоплено: \' + immovables.AccumulatedTotalFormatted">{{immovables.AccumulatedPercentageFormatted}}</td>\
            <td class="text-right" v-if="isfounder">\
                <span v-b-tooltip.hover :title="\'Добавить платёж 5%\'" class="fa-clickable" v-if="type===\'waiting\' || type===\'inQueue\' || type===\'bought\' " v-on:click="$emit(\'add-payment\', immovables,  5)"><i class="far fa-plus-square fa-fw"></i></span>\
                <span v-b-tooltip.hover :title="\'Добавить платёж 10%\'" class="fa-clickable" v-if="type===\'waiting\' || type===\'inQueue\' || type===\'bought\'" v-on:click="$emit(\'add-payment\', immovables, 10)"><i class="fas fa-plus-square fa-fw"></i></span>\
                <span v-b-tooltip.hover :title="\'Начать процесс покупки\'" class="fa-clickable" v-if="type===\'inQueue\' && accumulated >= immovables.Price" v-on:click="$emit(\'initiate-purchase\', immovables)"><i class="fal fa-fw fa-lock-alt"></i></span>\
                <span v-b-tooltip.hover :title="\'Завершить процесс покупки\'" class="fa-clickable" v-if="type===\'readyToBuy\'" v-on:click="$emit(\'mark-purchased\', immovables)"><i class="fal fa-fw fa-sack-dollar"></i></span>\
            </td></tr>'
    });

Vue.component('payment-item-before',
    {
        props: ['payment','index'],
        template: '<tr>\
            <td>{{payment.PaymentDateFormatted}}</td>\
            <template v-if="payment.EntranceFee && payment.EntranceFee.Amount > 0">\
            <td>вступительный взнос</td>\
            <td>{{payment.EntranceFee.AmountFormatted}}</td>\
            <td>&mdash;</td>\
            <td>&mdash;</td>\
            <td>&mdash;</td>\
            <td>&mdash;</td>\
            </template>\
            <template v-else>\
            <td>до покупки</td>\
            <td>{{payment.ImmovablesFundPart.AmountFormatted}}</td>\
            <td>{{payment.InflationDebt ? payment.InflationDebt.AmountFormatted : 0}}</td>\
            <td>&mdash;</td>\
            <td>&mdash;</td>\
            <td>{{payment.AccumulatedFormatted}}</td>\
            </template>\
            </tr>'
    });

Vue.component('payment-item-after',
    {
        props: ['payment','index'],
        template: '<tr>\
            <td>{{payment.PaymentDateFormatted}}</td>\
            <td>после покупки</td>\
            <td>{{payment.AmountFormatted}}</td>\
            <td>&mdash;</td>\
            <td>{{payment.InflationFundPart.AmountFormatted}}</td>\
            <td>{{payment.CoopFundPart.AmountFormatted}}</td>\
            <td>{{payment.ImmovablesFundPart.AmountFormatted}}</td>\
            </tr>'
    });

var app = new Vue({
    el: '#app',
    data: {
        m: {},
        loaded: false
    },
    computed: {
        totalImmovables: function(){
            return (this.m.NewImmovables ? this.m.NewImmovables.length : 0) + 
                (this.m.EntranceFeePaidImmovables ? this.m.EntranceFeePaidImmovables.length : 0) + 
                (this.m.InQueueImmovables ? this.m.InQueueImmovables.length : 0) + 
                (this.m.InPurchaseListImmovables ? this.m.InPurchaseListImmovables.length : 0) + 
                (this.m.PurchasedImmovables ? this.m.PurchasedImmovables.length : 0);
        },
        activeImmovables: function() {
            return this.totalImmovables - (this.m.NewImmovables ? this.m.NewImmovables.length : 0);
        }
    },
    methods: {
        displayMembers: function (action) {
            let currentObj = this;
            axios.post(action)
                .then(function (response) {
                    currentObj.m = response.data;
                    currentObj.loaded = true;
                })
                .catch(function (error) {
                    console.log(error);
                });
        },
        onAddPayment: function(immovables, paymentPercent) {
            let currentObj = this;
            axios.post('/CoOp/AddPayment/' + immovables.ImmovablesId + '/' + paymentPercent)
                .then(function (response) {
                    currentObj.m = response.data;
                    currentObj.$bvToast.toast('Payment added', {
                        title: 'Payment Added',
                        variant: 'success'
                    });
                })
                .catch(function (error) {
                    currentObj.$bvToast.toast(error, {
                        title: 'Error',
                        variant: 'danger'
                    });
                });
        },
        onInitiateImmovablesPurchase: function(immovables) {
            let currentObj = this;
            axios.post('/CoOp/InitiateImmovablesPurchase/' + immovables.ContractNo)
                .then(function (response) {
                    currentObj.m = response.data;
                    currentObj.loaded = true;
                    currentObj.$bvToast.toast('Purchase initiated', {
                        title: 'Purchase initiated',
                        variant: 'success'
                    });
                })
                .catch(function (error) {
                    currentObj.$bvToast.toast(error, {
                        title: 'Error',
                        variant: 'danger'
                    });
                });
        },
        onMarkImmovablesPurchased: function(immovables) {
            let currentObj = this;
            axios.post('/CoOp/MarkImmovablesPurchased/' + immovables.ContractNo)
                .then(function (response) {
                    currentObj.m = response.data;
                    currentObj.loaded = true;
                    currentObj.$bvToast.toast('Purchase completed', {
                        title: 'Immovables marked as purchased',
                        variant: 'success'
                    });
                })
                .catch(function (error) {
                    currentObj.$bvToast.toast(error, {
                        title: 'Error',
                        variant: 'danger'
                    });
                });
            
        },
        onImpersonate: function(immovables) {
            window.location = '/CoOp/ImpersonateExistingMember/' + immovables.MemberId;
        }


    },
    created: function() {
        this.displayMembers('/Coop/DisplayMembers');
    }
    //,
    //created: function () {
    //    let self = this;
    //    fetch('/Calc/GetCalculations',
    //        {
    //            method: 'POST',
    //            headers: {
    //                'Content-Type': 'application/json;charset=UTF-8'
    //            },
    //            body: JSON.stringify({
    //                'immovablesValue': this.immovablesValue,
    //                'interestRate': this.interestRate,
    //                'termInYears': this.termInYears
    //            })
    //        })
    //        .then(response => response.json())
    //        .then(data => {
    //            app.paymentList = data;
    //        });
    //}

});


