
var app = new Vue({
    el: '#app',
    data: {
        m: {
            DocumentType: null
        },
        Errors: {},
        DocumentType: [
            { value: 'null', text: 'Выбрать...' },
            { value: 'passport_rb', text: 'Паспорт РБ' },
            { value: 'vid_rb', text: 'Вид на жительство РБ' }
        ],
        loaded: false
    },
   
    methods: {
        registerMember: function () {
            let currentObj = this;
            axios.post('/Coop/RegisterMember',  currentObj.m )
                .then(function (response) {
                    currentObj.loaded = true;
                    currentObj.m = response.data;
                })
                .catch(function (error) {
                    currentObj.loaded = true;
                    currentObj.Errors = error.response.data;
                });
        },
        validationState: function (propertyName) {
            if (!this.loaded)
                return null;
            return this.Errors[propertyName] === undefined;
        }
    }
});


