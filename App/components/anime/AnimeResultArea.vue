<template>
    <v-layout>
      <v-flex>
        <div>
          <v-btn raised large color="error" class="optionButton" v-on:click="resetList" :disabled="charactersData.length < 1">Reset</v-btn>
          <v-btn depressed large color="primary" class="optionButton" v-on:click="computeResults" :disabled="charactersData.length < 2">Compare</v-btn>
          <v-btn depressed large color="secondary" class="optionButton" v-on:click="generateShareLink" :disabled="!showTables || charactersData.length < 2">Share Link</v-btn>
        </div>
        <div>
          <anime-table-selection 
            v-if="showTables" 
            :charactersData="outputData" 
            :counter="counter" 
            :animeData="animeData"/>
        </div>
      </v-flex>
      <share-link-snackbar
        :showSnackbar="snackbar"
        @hideSnackBar="snackbar = false"/>
    </v-layout>
</template>

<script>
import AnimeTableSelection from '@/components/anime/AnimeTableSelection.vue'
import ShareLinkSnackbar from '@/components/shared/ui-components/ShareLinkSnackbar.vue';

export default {
  name: 'AnimeResultArea',
  components: {
    'anime-table-selection': AnimeTableSelection,
    'share-link-snackbar': ShareLinkSnackbar
  },
  props: {
    charactersData: {
      type: Array,
      required: false
    },
    animeData: {
      type: Array,
      required: false
    },
    runImmediately: {
      type: Boolean,
      required: true
    }
  },
  data () {
    return {
      showTables: false,
      counter: 0,
      outputData: [],
      snackbar: false
    }
  },
  methods: {
    resetList () {
      this.$emit('resetList')
    },
    computeResults () {
      this.outputData = []
      var filteredData = new Array(this.charactersData.length)
      var partialResults = new Array(this.charactersData.length)

      for (var i = 0; i < this.charactersData.length; i++) {
        partialResults[i] = []
        filteredData[i] = []
      }

      for (var k = 0; k < this.animeData.length; k++) {
        for (i = 0; i < this.charactersData[k].length; i++) {
          for (var j = 0; j < this.charactersData[k][i].voice_actors.length; j++) {
            if (this.charactersData[k][i].voice_actors[j].language === 'Japanese') {
              filteredData[k].push({
                seiyuu: this.charactersData[k][i].voice_actors[j],
                roles: [{
                  anime: this.animeData[k].title,
                  character: this.charactersData[k][i]
                }]
              })
            }
          }
        }
      }

      partialResults[0] = filteredData[0]

      for (var animeIndex = 1; animeIndex < filteredData.length; animeIndex++) {
        for (i = 0; i < filteredData[animeIndex].length; i++) {
          for (j = 0; j < partialResults[animeIndex - 1].length; j++) {
            if (partialResults[animeIndex - 1][j].seiyuu.mal_id === filteredData[animeIndex][i].seiyuu.mal_id) {
              // Weird deep clone of object
              var cloneObject = JSON.parse(JSON.stringify(partialResults[animeIndex - 1][j]))
              partialResults[animeIndex].push(cloneObject)
              partialResults[animeIndex][partialResults[animeIndex].length - 1].roles.push({
                anime: this.animeData[animeIndex].title,
                character: filteredData[animeIndex][i].roles[0].character
              })
            }
          }
        }
        partialResults[animeIndex] = partialResults[animeIndex].filter(x => x.roles.length === (animeIndex + 1))
      }
      this.outputData = partialResults[this.charactersData.length - 1]
      this.counter++
      this.showTables = true
    },
    generateShareLink () {
      var animeIds = ''
      this.animeData.forEach(element => {
        animeIds += element.malId + ';'
      });
      
      animeIds = animeIds.slice(0, -1)
      animeIds = this.encodeURL(animeIds)

      var shareLink = process.env.baseUrl + $nuxt.$route.path.toLowerCase() + '?animeIds=' + animeIds

      this.$copyText(shareLink)
      this.snackbar = true
    }
  },
  watch: {
    charactersData: function (newVal, oldVal) {
      if (this.charactersData.length === 0) {
        this.showTables = false
      }
    },
    runImmediately: function (val) {
      if (val === true) {
        this.computeResults()
      }
    }
  }
}
</script>

<style>
img.miniav {
    max-height: 98px;
    max-width: 63px;
    width: auto;
    height: auto;
}

img.av {
    max-height: 140px;
    max-width: 90px;
    width: auto;
    height: auto;
}
</style>