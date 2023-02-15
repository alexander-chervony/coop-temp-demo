
var app = new Vue({
    el: '#app',
    data: {
        m: {
            DocumentType: null,
            LivingIn:null,
            MaritalStatus:null,
            HaveACar: null,
            HaveAProperty: null,
            HaveActiveCredit: null
        },
        Errors: {},
        DocumentType: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'passport_rb', text: 'Паспорт РБ' },
            { value: 'vid_rb', text: 'Вид на жительство РБ' }
        ],
        LivingIn: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'native', text: 'Своё' },
            { value: 'parents', text: 'С родителями' },
            { value: 'rent', text: 'аренда' }
        ],
        MaritalStatus: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'married', text: 'женат/замужем' },
            { value: 'single', text: 'не женат/не замужем' }
        ],
        HaveACar: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'true', text: 'Есть' },
            { value: 'false', text: 'Нет' }
        ],
        HaveAProperty: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'true', text: 'Есть' },
            { value: 'false', text: 'Нет' }
        ],
        HaveActiveCredit: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'true', text: 'Есть' },
            { value: 'false', text: 'Нет' }
        ],
        loaded: false
    },
   
    methods: {
        sendApplication: function () {
            let currentObj = this;
            axios.post('/ApplyForCredit/SendApplication',  currentObj.m )
                .then(function (response) {
                    currentObj.loaded = true;
                    currentObj.Errors = {};
                    currentObj.m = response.data;
                    currentObj.$bvToast.toast('Анкета отправлена', {
                        title: 'Вы - восхитетельны! Анкета отправлена успешно',
                        variant: 'success',
                        delay: 5000
                    });
                })
                .catch(function (error) {
                    currentObj.loaded = true;
                    currentObj.Errors = error.response.data;
                    currentObj.$bvToast.toast('Исправьте ошибки заполнения анкеты', {
                        title: 'Анкета НЕ отправлена',
                        variant: 'danger',
                        delay: 5000
                    });
                });
        },
        validationState: function (propertyName) {
            if (!this.loaded)
                return null;
            return this.Errors[propertyName] === undefined;
        }
    }
});


