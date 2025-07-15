<template>
  <div class="card mx-auto" style="max-width: 400px;">
    <img :src="pokemon.sprites.front_default"
         :alt="pokemon.name"
         class="card-img-top mx-auto mt-3"
         style="width: 150px; height: 150px;" />
    <div class="card-body text-center">
      <h5 class="card-title text-capitalize">{{ pokemon.name }}</h5>
      <ul class="list-group list-group-flush text-start">
        <li class="list-group-item"><strong>Height:</strong> {{ pokemon.height }}</li>
        <li class="list-group-item"><strong>Weight:</strong> {{ pokemon.weight }}</li>
        <li class="list-group-item">
          <strong>Types:</strong> {{ pokemon.types.map(t => t.type.name).join(', ') }}
        </li>
        <li class="list-group-item">
          <strong>Abilities:</strong> {{ pokemon.abilities.map(a => a.ability.name).join(', ') }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
  defineProps({
    pokemon: {
      type: Object,
      required: true,
    },
  })
</script>
