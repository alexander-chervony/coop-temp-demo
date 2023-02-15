Vue.component('payment-item',
    {
        props: ['p'],
        methods:{
            colorifyPayment: function(payment){
                if( payment.Number === 0)
                    return "";
                if( payment.IsAccumulation === true)
                    return "text-success";
                return "text-danger";
            }
        },
        template: '<tr><th>{{ p.Number }}</th><td class="d-none">{{ p.Date }}</td><td>{{ p.ValueBodyString }}</td><td :class="colorifyPayment(p)" v-b-tooltip.hover :title="p.IsAccumulation===true?\'начислено на мои накопления, следующий накопительный платёж уменьшен\':\'взнос за владение и пользование\'">{{ p.ValuePercentString }}</td><td>{{ p.ValueTotalString }}</td></tr>'
    });

var app = new Vue({
    el: '#app',
    data: {
        ImmovablesValue: 70000,
        TermInYears: 20,
        BankInterestRate: 15,
        CoopInflationRate: 5,
        CoopInterestRateString: "",
        AccumulationTime: 0,

        BankAdditionalFeeString: "",
        BankTotalPayments: 0,
        BankMonthlyPayments: [],

        CoopOverPaymentString:"",
        BankOverPaymentString:"",
        OverPaymentTimesString:"",

        CoopEntranceFeeString: "",
        CoopTotalPayments:0,
        CoopMonthlyPayments: []
    },
    //computed: {
    //    totalPaid: function () {
    //        return this.paymentList.reduce((partial, a) => partial + a.ValueTotal, 0);
    //    }
    //},
    methods: {
        sliderChange: function () {
            let currentObj = this;
            axios.post('/Calc/Calculate',
                {
                    ImmovablesValue: this.ImmovablesValue,
                    TermInYears: this.TermInYears,
                    BankInterestRate: this.BankInterestRate,
                    CoopInflationRate: this.CoopInflationRate,
                    AccumulationTime: this.AccumulationTime
                }
            )
                .then(function (response) {
                    currentObj.BankAdditionalFeeString = response.data.BankAdditionalFeeString;
                    currentObj.BankTotalPayments = response.data.BankTotalPayments;
                    currentObj.BankMonthlyPayments = response.data.BankMonthlyPayments;
                    currentObj.CoopEntranceFeeString = response.data.CoopEntranceFeeString;
                    currentObj.CoopTotalPayments = response.data.CoopTotalPayments;
                    currentObj.CoopMonthlyPayments = response.data.CoopMonthlyPayments;
                    currentObj.CoopInterestRateString = response.data.CoopInterestRateString;
                    currentObj.CoopOverPaymentString = response.data.CoopOverPaymentString;
                    currentObj.BankOverPaymentString = response.data.BankOverPaymentString;
                    currentObj.OverPaymentTimesString = response.data.OverPaymentTimesString;
                })
                .catch(function (error) {
                    console.log(error);
                });
        }
    },
    created: function() {
        this.AccumulationTime = Math.round(this.TermInYears*12/5);
        this.sliderChange();
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


